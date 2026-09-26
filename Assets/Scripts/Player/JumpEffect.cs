using UnityEngine;
using UnityEngine.InputSystem;

namespace CheeseDash
{
    [AddComponentMenu("Cheese Dash/Jump Effect")]
    [RequireComponent(typeof(AudioSource))]
    public class JumpEffect : MonoBehaviour
    {
        [SerializeField] private AudioClip mouseSound;
        [SerializeField] private bool repeatWhileHeld;
        [SerializeField] private float repeatDelay = 0.35f;

        private AudioSource _source;
        private float _nextPlayTime;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();

            _source.playOnAwake = false;
            _source.loop = false;
        }

        private void Update()
        {
            if (Keyboard.current == null || mouseSound == null) return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Play();
            }
            else if (repeatWhileHeld && Keyboard.current.spaceKey.isPressed && Time.time >= _nextPlayTime)
            {
                Play();
            }
        }

        private void Play()
        {
            _source.PlayOneShot(mouseSound);
            _nextPlayTime = Time.time + repeatDelay;
        }
    }
}
