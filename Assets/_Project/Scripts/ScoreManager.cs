using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private float pointsPerSecond = 10f;

    private float score;
    private int highScore;

    private void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (highScoreText != null)
            highScoreText.text = $"High: {highScore}";
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        score += pointsPerSecond * Time.deltaTime;
        int s = Mathf.FloorToInt(score);

        if (scoreText != null)
            scoreText.text = $"Score: {s}";

        if (s > highScore)
        {
            highScore = s;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();

            if (highScoreText != null)
                highScoreText.text = $"High: {highScore}";
        }
    }
}