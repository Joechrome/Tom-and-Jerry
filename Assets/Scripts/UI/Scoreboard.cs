using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CheeseDash
{
    [AddComponentMenu("Cheese Dash/Scoreboard")]
    public class Scoreboard : MonoBehaviour
    {
        [SerializeField] private int framesPerPoint = 10;
        [SerializeField] private string label = "SCORE";

        private TMP_Text _tmpText;
        private Text _uiText;
        private double _frameAccumulator;
        private long _frames;
        private int _score;
        private bool _running = true;

        public int Score => _score;
        public long Frames => _frames;
        public bool IsRunning => _running;
        public bool NormaliseFrameRate { get; set; } = false;
        public float ReferenceFramesPerSecond { get; set; } = 60f;

        public event System.Action<int> ScoreChanged;

        public void SetRunning(bool running)
        {
            _running = running;
        }

        public void ResetScore()
        {
            _frameAccumulator = 0d;
            _frames = 0;
            _score = 0;
            RefreshText();
            ScoreChanged?.Invoke(_score);
        }

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            if (_tmpText == null) _tmpText = GetComponentInChildren<TMP_Text>();

            if (_tmpText == null)
            {
                _uiText = GetComponent<Text>();
                if (_uiText == null) _uiText = GetComponentInChildren<Text>();
            }

            if (_tmpText == null && _uiText == null)
            {
                Debug.LogError("[Scoreboard] This component has to sit on a text object. " +
                               "Add it to a TextMeshPro text (or a UI Text) and it will use that.",
                               this);
                enabled = false;
                return;
            }

            RefreshText();
        }

        private void Update()
        {
            if (!_running) return;
            if (Time.timeScale <= 0f) return;

            _frameAccumulator += NormaliseFrameRate
                ? Time.deltaTime * ReferenceFramesPerSecond
                : 1d;

            _frames = (long)_frameAccumulator;

            int newScore = (int)(_frames / Mathf.Max(1, framesPerPoint));
            if (newScore == _score) return;

            _score = newScore;
            RefreshText();
            ScoreChanged?.Invoke(_score);
        }

        private void OnValidate()
        {
            framesPerPoint = Mathf.Max(1, framesPerPoint);
        }

        private void RefreshText()
        {
            string value = string.IsNullOrEmpty(label)
                ? _score.ToString()
                : $"{label} {_score}";

            if (_tmpText != null) _tmpText.text = value;
            else if (_uiText != null) _uiText.text = value;
        }
    }
}
