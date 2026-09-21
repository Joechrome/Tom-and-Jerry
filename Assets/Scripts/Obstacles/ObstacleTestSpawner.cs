using System.Collections.Generic;
using UnityEngine;

namespace CheeseDash
{
    /// <summary>
    /// TEMPORARY TEST HARNESS - not the real spawner.
    ///
    /// Drop this on any empty GameObject in a scene and press Play to see both obstacle
    /// types scrolling toward the player:
    ///
    ///   Short - the narrow bottle lying horizontally, 0.45 units tall  -> small hop
    ///   High  - the widest bottle standing upright,   1.07 units tall  -> full jump
    ///
    /// Andrew's real ObstacleSpawner replaces this. When that exists, delete this file.
    /// It deliberately does not reference WorldSpeed or GameManager yet, so it compiles and
    /// runs on its own before those systems are written.
    /// </summary>
    [AddComponentMenu("Cheese Dash/Obstacle Test Spawner (temporary)")]
    public class ObstacleTestSpawner : MonoBehaviour
    {
        [Header("Sprites (auto-filled in the editor if left empty)")]
        [Tooltip("The widest bottle from the condiments sheet. Spawned as the High obstacle.")]
        [SerializeField] private Sprite highSprite;

        [Tooltip("The narrow bottle, already rotated horizontal. Spawned as the Short obstacle.")]
        [SerializeField] private Sprite shortSprite;

        [Header("Obstacle behaviour")]
        [Tooltip("Leave as Grounded to see the two different heights side by side. " +
                 "Switch the High obstacle to Overhead to preview a crouch-under hazard instead.")]
        [SerializeField] private ObstaclePlacement highPlacement = ObstaclePlacement.Grounded;

        [Tooltip("Only used when the High obstacle is Overhead: the gap left at foot level.")]
        [SerializeField] private float clearance = 0.45f;

        [Header("Scrolling")]
        [Tooltip("How fast obstacles travel toward the player, in world units per second.")]
        [SerializeField] private float scrollSpeed = 5f;

        [Tooltip("World Y of the ground line.")]
        [SerializeField] private float groundY = 0f;

        [Header("Spawning")]
        [Tooltip("Seconds between spawns.")]
        [SerializeField] private float spawnInterval = 1.6f;

        [Tooltip("Random extra delay added to each interval, so the rhythm is not perfectly mechanical.")]
        [SerializeField] private float spawnIntervalJitter = 0.4f;

        [Tooltip("World X where obstacles appear. Must be off-screen to the right.")]
        [SerializeField] private float spawnX = 12f;

        [Tooltip("World X where obstacles are recycled. Should be off-screen to the left.")]
        [SerializeField] private float despawnX = -14f;

        [Tooltip("Alternate High / Short. Turn off to spawn only the short one, for inspecting one sprite.")]
        [SerializeField] private bool alternateTypes = true;

        private readonly List<GameObject> _live = new List<GameObject>();
        private float _timer;
        private bool _nextIsHigh;

        private void Reset()
        {
            // Convenience: fill the sprite slots automatically when the component is first
            // added, so this works with no manual wiring. Editor-only.
#if UNITY_EDITOR
            if (highSprite == null) highSprite = LoadSprite("Assets/Art/Sprites/Obstacles/obstacle_high.png");
            if (shortSprite == null) shortSprite = LoadSprite("Assets/Art/Sprites/Obstacles/obstacle_short.png");
#endif
        }

        private void Update()
        {
            SpawnOnTimer();
            ScrollLiveObstacles();
        }

        private void SpawnOnTimer()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = spawnInterval + Random.Range(0f, spawnIntervalJitter);

            if (highSprite == null && shortSprite == null)
            {
                Debug.LogWarning("[ObstacleTestSpawner] No sprites assigned - nothing to spawn.", this);
                enabled = false;
                return;
            }

            bool spawnHigh = alternateTypes && _nextIsHigh;
            _nextIsHigh = !_nextIsHigh;

            Sprite chosen = spawnHigh ? highSprite : shortSprite;
            ObstacleHeight type = spawnHigh ? ObstacleHeight.High : ObstacleHeight.Short;
            ObstaclePlacement placement = spawnHigh ? highPlacement : ObstaclePlacement.Grounded;

            // If the requested sprite is missing, fall back to whichever one exists.
            if (chosen == null)
            {
                chosen = spawnHigh ? shortSprite : highSprite;
                type = spawnHigh ? ObstacleHeight.Short : ObstacleHeight.High;
                placement = ObstaclePlacement.Grounded;
            }

            Spawn(chosen, type, placement);
        }

        private void Spawn(Sprite sprite, ObstacleHeight type, ObstaclePlacement placement)
        {
            var go = new GameObject($"{type.DisplayName()} {placement.DisplayName()} Obstacle");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;

            // Adding Obstacle pulls in the BoxCollider2D and Rigidbody2D it requires, then
            // sizes the collider and places the object at the right height.
            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Configure(type, groundY, placement);

            // Tune the crouch gap only where it matters.
            if (placement == ObstaclePlacement.Overhead)
            {
                obstacle.SetClearance(clearance);
            }

            go.transform.position = new Vector3(spawnX, go.transform.position.y, 0f);

            _live.Add(go);
        }

        private void ScrollLiveObstacles()
        {
            float step = scrollSpeed * Time.deltaTime;

            for (int i = _live.Count - 1; i >= 0; i--)
            {
                GameObject go = _live[i];

                if (go == null)
                {
                    _live.RemoveAt(i);
                    continue;
                }

                go.transform.position += Vector3.left * step;

                if (go.transform.position.x <= despawnX)
                {
                    // The real spawner will use a pool instead of Destroy, to avoid
                    // garbage-collection hitches. That matters on WebGL builds.
                    Destroy(go);
                    _live.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        private static Sprite LoadSprite(string path)
        {
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite) return sprite;
            }
            return null;
        }
#endif

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawLine(new Vector3(despawnX, groundY, 0f), new Vector3(spawnX, groundY, 0f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(spawnX, groundY - 0.5f, 0f), new Vector3(spawnX, groundY + 3f, 0f));
            Gizmos.DrawLine(new Vector3(despawnX, groundY - 0.5f, 0f), new Vector3(despawnX, groundY + 3f, 0f));
        }
    }
}
