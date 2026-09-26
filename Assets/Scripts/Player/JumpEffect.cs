using UnityEngine;
using UnityEngine.InputSystem;

namespace CheeseDash
{
    [AddComponentMenu("Cheese Dash/Jump Effect")]
    [RequireComponent(typeof(AudioSource))]
    public class JumpEffect : MonoBehaviour
    {
        [SerializeField] private AudioClip mouseSound;

        [Tooltip("Off: one sound each time space goes down. " +
                 "On: the sound repeats for as long as space is held down.")]
        [SerializeField] private bool repeatWhileHeld = false;

        [SerializeField] private float repeatDelay = 0.35f;

        private AudioSource _source;
        private float _nextPlayTime;

        private void Reset()
        {
#if UNITY_EDITOR
            if (mouseSound == null)
            {
                mouseSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/Art/Audio/mouse sound.wav");
            }
#endif
        }

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();

            _source.playOnAwake = false;
            _source.loop = false;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Play();
                _nextPlayTime = Time.time + repeatDelay;
                return;
            }

            if (!repeatWhileHeld) return;
            if (!Keyboard.current.spaceKey.isPressed) return;
            if (Time.time < _nextPlayTime) return;

            Play();
            _nextPlayTime = Time.time + repeatDelay;
        }

        private void Play()
        {
            if (mouseSound == null) return;
            _source.PlayOneShot(mouseSound);
        }
    }
}
