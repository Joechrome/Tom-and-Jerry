using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CheeseDash
{
    [AddComponentMenu("Cheese Dash/Scoreboard")]
    public class Scoreboard : MonoBehaviour
    {
        private const float ReferenceFrameRate = 60f;

        [SerializeField] private int framesPerPoint = 10;
        [SerializeField] private string label = "SCORE";
        public float winScore;

        private TMP_Text _text;
        private double _frames;
        private int _score;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (_text == null) _text = GetComponentInChildren<TMP_Text>();

            if (_text == null)
            {
                enabled = false;
                return;
            }

            Show();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            _frames += Time.deltaTime * ReferenceFrameRate;

            int score = (int)(_frames / Mathf.Max(1, framesPerPoint));
            if (score == _score) return;

            _score = score;
            Show();

            if (winScore > 0f && _score >= winScore)
            {
                SceneManager.LoadScene("Win Screen");
            }
        }

        private void Show()
        {
            _text.text = string.IsNullOrEmpty(label) ? _score.ToString() : $"{label} {_score}";
        }
    }
}
