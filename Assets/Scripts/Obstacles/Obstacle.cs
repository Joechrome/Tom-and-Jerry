using UnityEngine;

namespace CheeseDash
{
    /// <summary>
    /// Marks a GameObject as a dodgeable obstacle and keeps its collider, its artwork and
    /// its vertical position consistent with each other.
    ///
    /// The object's origin is the CENTRE of its sprite (pivot 0.5, 0.5), so all the maths
    /// below works in terms of the collider's bottom edge. You never position these by
    /// hand: pick the height and placement in the Inspector and the component snaps
    /// itself to the right spot.
    ///
    /// Attach to a GameObject that has a SpriteRenderer with the obstacle sprite on it.
    /// The BoxCollider2D and Rigidbody2D are added automatically.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Obstacle : MonoBehaviour
    {
        [Header("Type")]
        [Tooltip("Short = the narrow horizontal sprite, a small hop. " +
                 "High = the wide upright sprite, a full-height jump.")]
        [SerializeField] private ObstacleHeight height = ObstacleHeight.Short;

        [Tooltip("Grounded = sits on the ground and must be jumped. " +
                 "Overhead = hangs above the ground and must be crouched under.")]
        [SerializeField] private ObstaclePlacement placement = ObstaclePlacement.Grounded;

        [Header("Placement (world units)")]
        [Tooltip("World Y of the ground the player runs along.")]
        [SerializeField] private float groundY = 0f;

        [Tooltip("Only used when placement is Overhead: the clear gap left at foot level so " +
                 "a crouching player fits underneath. Must be at least the player's crouching " +
                 "height, and less than their standing height, or the crouch is pointless.")]
        [SerializeField] private float clearance = 0.45f;

        [Tooltip("Snap this object to the correct height automatically. Turn off if a parent " +
                 "or a spawner controls the position.")]
        [SerializeField] private bool snapVertically = true;

        [Header("Player metrics (used for validation only - they move nothing)")]
        [Tooltip("How tall the player is while crouching. An Overhead obstacle must leave at least this much room.")]
        [SerializeField] private float playerCrouchHeight = 0.3f;

        [Tooltip("How tall the player is standing up. An Overhead obstacle must be lower than this, " +
                 "otherwise the player can walk straight under it without crouching.")]
        [SerializeField] private float playerStandingHeight = 0.6f;

        [Tooltip("How high the player's jump reaches, measured from the ground. " +
                 "A grounded obstacle taller than this is impossible to clear.")]
        [SerializeField] private float playerJumpApex = 1.4f;

        [Header("Collider")]
        [Tooltip("Resize the BoxCollider2D to match the sprite automatically.")]
        [SerializeField] private bool autoFitCollider = true;

        [Tooltip("Extra collider size. Use a negative value to make the hitbox slightly " +
                 "more forgiving than the artwork.")]
        [SerializeField] private Vector2 colliderPadding = Vector2.zero;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        private Rigidbody2D _body;

        // ---------------------------------------------------------------- public API

        public ObstacleHeight Height => height;
        public ObstaclePlacement Placement => placement;

        /// <summary>Jump for a grounded obstacle, Crouch for an overhead one.</summary>
        public DodgeAction RequiredAction => placement.RequiredAction();

        /// <summary>World Y of the ground this obstacle sits on.</summary>
        public float GroundY => groundY;

        /// <summary>World Y of the bottom edge of the hitbox.</summary>
        public float BottomY => transform.position.y + _collider.offset.y - (_collider.size.y * 0.5f);

        /// <summary>World Y of the top edge of the hitbox.</summary>
        public float TopY => transform.position.y + _collider.offset.y + (_collider.size.y * 0.5f);

        /// <summary>Height of the hitbox in world units.</summary>
        public float HitboxHeight => _collider.size.y;

        /// <summary>How high the player must jump to clear this, measured from the ground.</summary>
        public float HeightAboveGround => TopY - groundY;

        /// <summary>
        /// Set the type, ground line and placement from code (e.g. from a spawner), then
        /// re-apply the layout. Use this instead of poking the serialized fields directly.
        /// </summary>
        public void Configure(ObstacleHeight newHeight, float newGroundY,
                              ObstaclePlacement newPlacement = ObstaclePlacement.Grounded)
        {
            height = newHeight;
            groundY = newGroundY;
            placement = newPlacement;
            CacheComponents();
            ApplyLayout();
        }

        /// <summary>
        /// Set the crouch gap for an Overhead obstacle. Only meaningful when
        /// <see cref="Placement"/> is Overhead.
        /// </summary>
        public void SetClearance(float newClearance)
        {
            clearance = Mathf.Max(0f, newClearance);
            ApplyLayout();
        }

        /// <summary>
        /// Fit the collider to the sprite, then place the object so the bottom edge of its
        /// hitbox sits at the height its placement calls for.
        /// </summary>
        public void ApplyLayout()
        {
            CacheComponents();
            if (_spriteRenderer == null || _collider == null) return;

            if (autoFitCollider && _spriteRenderer.sprite != null)
            {
                Vector2 spriteSize = SpriteWorldSize();
                _collider.size = new Vector2(
                    Mathf.Max(0.01f, spriteSize.x + colliderPadding.x),
                    Mathf.Max(0.01f, spriteSize.y + colliderPadding.y));
                _collider.offset = Vector2.zero;
            }

            if (snapVertically)
            {
                float bottomTarget = placement == ObstaclePlacement.Overhead
                    ? groundY + clearance   // leave a gap at foot level to crouch through
                    : groundY;              // sit flat on the ground

                // The origin is the sprite's centre, so lift by half the collider height to
                // put the collider's BOTTOM edge exactly on bottomTarget.
                Vector3 p = transform.position;
                p.y = bottomTarget + (_collider.size.y * 0.5f) - _collider.offset.y;
                transform.position = p;
            }
        }

        // ---------------------------------------------------------------- Unity callbacks

        private void Awake()
        {
            CacheComponents();

            // Obstacles are moved by scripts (the world scrolls toward the player), not by
            // physics. A Kinematic body tells Unity 2D that this collider moves, which keeps
            // collision detection correct and cheap. Without it, moving colliders behave like
            // static geometry and hit detection gets unreliable.
            if (_body != null)
            {
                _body.bodyType = RigidbodyType2D.Kinematic;
                _body.simulated = true;
            }

            ApplyLayout();
        }

        private void Reset()
        {
            CacheComponents();
            ApplyLayout();
        }

        private void OnValidate()
        {
            // Keep the numbers sane before they reach the layout maths.
            clearance = Mathf.Max(0f, clearance);
            playerCrouchHeight = Mathf.Max(0.01f, playerCrouchHeight);
            playerStandingHeight = Mathf.Max(playerCrouchHeight + 0.01f, playerStandingHeight);
            playerJumpApex = Mathf.Max(0.01f, playerJumpApex);

            CacheComponents();
            ApplyLayout();
            Validate();
        }

        // ---------------------------------------------------------------- helpers

        private void CacheComponents()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_collider == null) _collider = GetComponent<BoxCollider2D>();
            if (_body == null) _body = GetComponent<Rigidbody2D>();
        }

        private Vector2 SpriteWorldSize()
        {
            Vector3 s = _spriteRenderer.sprite.bounds.size;
            Vector3 scale = transform.lossyScale;
            return new Vector2(Mathf.Abs(s.x * scale.x), Mathf.Abs(s.y * scale.y));
        }

        /// <summary>
        /// Warn about setups that make an obstacle impossible, or make it pointless. These
        /// are the mistakes that turn into "this game is unfair" reports later, so they are
        /// worth catching in the Inspector instead.
        /// </summary>
        private void Validate()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

            if (placement == ObstaclePlacement.Overhead)
            {
                if (clearance < playerCrouchHeight)
                {
                    Debug.LogWarning(
                        $"[Obstacle] '{name}' is Overhead but its clearance ({clearance:0.##}) is smaller " +
                        $"than the player's crouching height ({playerCrouchHeight:0.##}). A crouching player " +
                        "still cannot fit through - this obstacle is impossible.", this);
                }

                if (clearance >= playerStandingHeight)
                {
                    Debug.LogWarning(
                        $"[Obstacle] '{name}' is Overhead but its clearance ({clearance:0.##}) is not lower " +
                        $"than the player's standing height ({playerStandingHeight:0.##}). The player can run " +
                        "straight under it without crouching, so the crouch is never actually needed.", this);
                }
            }
            else
            {
                float top = HeightAboveGround;

                if (top > playerJumpApex)
                {
                    Debug.LogWarning(
                        $"[Obstacle] '{name}' stands {top:0.##} units tall but the player's jump only reaches " +
                        $"{playerJumpApex:0.##}. This obstacle cannot be cleared. Shrink the sprite, lower the " +
                        "collider, or raise the jump.", this);
                }
                else if (top > playerJumpApex - 0.05f)
                {
                    Debug.LogWarning(
                        $"[Obstacle] '{name}' is {top:0.##} units tall against a jump of {playerJumpApex:0.##} " +
                        "- that is almost no margin. Players will call it unfair.", this);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            CacheComponents();
            if (_collider == null) return;

            Vector3 centre = transform.position;
            float halfWidth = Mathf.Max(1f, _collider.size.x * 0.5f) + 0.5f;

            // The ground line.
            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            Gizmos.DrawLine(new Vector3(centre.x - halfWidth, groundY, 0f),
                            new Vector3(centre.x + halfWidth, groundY, 0f));

            if (placement == ObstaclePlacement.Overhead)
            {
                // The gap the player crouches through.
                Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.35f);
                float gapTop = groundY + clearance;
                Gizmos.DrawCube(new Vector3(centre.x, (groundY + gapTop) * 0.5f, 0f),
                                new Vector3(halfWidth * 2f, Mathf.Max(0.001f, clearance), 0.01f));

                Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 1f);
                Gizmos.DrawLine(new Vector3(centre.x - halfWidth, gapTop, 0f),
                                new Vector3(centre.x + halfWidth, gapTop, 0f));
            }
            else
            {
                // The height the player has to clear.
                Gizmos.color = new Color(1f, 0.75f, 0.1f, 1f);
                Gizmos.DrawLine(new Vector3(centre.x - halfWidth, TopY, 0f),
                                new Vector3(centre.x + halfWidth, TopY, 0f));
            }

            // The player's jump ceiling, for eyeballing whether this is clearable.
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            Gizmos.DrawLine(new Vector3(centre.x - halfWidth, groundY + playerJumpApex, 0f),
                            new Vector3(centre.x + halfWidth, groundY + playerJumpApex, 0f));

            // The hitbox.
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 1f);
            Gizmos.DrawWireCube(transform.position + (Vector3)_collider.offset, (Vector3)_collider.size);
        }
    }
}
