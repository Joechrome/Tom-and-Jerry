using UnityEngine;
using UnityEngine.UI;

namespace CheeseDash
{
    [AddComponentMenu("Cheese Dash/Scoreboard")]
    public class Scoreboard : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private int framesPerPoint = 10;
        [SerializeField] private string label = "SCORE";

        [Header("Display")]
        [SerializeField] private Vector2 position = new Vector2(0f, 0f);
        [SerializeField] private int textSize = 36;
        [SerializeField] private Font fontOverride;

        private Text _text;
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
            BuildUI();
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
            textSize = Mathf.Max(1, textSize);
            ApplyDisplaySettings();
        }

        private void BuildUI()
        {
            var canvasGO = new GameObject("Scoreboard Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = ResolveFont();
            if (font == null)
            {
                Debug.LogError(
                    "[Scoreboard] No font could be found, so the score will not be drawn. " +
                    "Fix: select the GameObject holding this component and assign any Font to " +
                    "the 'Font Override' slot. If you have no font asset, make one with " +
                    "Assets > Create > Text > Font.", this);
                return;
            }

            var textGO = new GameObject("Score Text",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGO.transform.SetParent(canvasGO.transform, false);

            _text = textGO.GetComponent<Text>();
            _text.font = font;

            if (font.material != null) _text.material = font.material;

            _text.alignment = TextAnchor.UpperRight;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
            _text.raycastTarget = false;
            _text.rectTransform.sizeDelta = new Vector2(600f, 120f);

            ApplyDisplaySettings();
        }

        private void ApplyDisplaySettings()
        {
            if (_text == null) return;

            _text.fontSize = textSize;

            RectTransform rect = _text.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-position.x, -position.y);
        }

        private Font ResolveFont()
        {
            if (fontOverride != null) return fontOverride;

            Font font = null;

            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { }

            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { }
            }

            if (font == null)
            {
                Font[] loaded = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font candidate in loaded)
                {
                    if (candidate != null)
                    {
                        font = candidate;
                        break;
                    }
                }
            }

            return font;
        }

        private void RefreshText()
        {
            if (_text == null) return;

            _text.text = string.IsNullOrEmpty(label)
                ? _score.ToString()
                : $"{label} {_score}";
        }
    }
}
