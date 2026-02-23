using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("PlayerPrefs key")]
    [SerializeField] private string leaderboardKey = "LeaderboardTop5";

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText; // HUD w trakcie gry
    [SerializeField] private TMP_Text top5Text;  // tekst na GameOverPanel

    [Header("Scoring")]
    [SerializeField] private float pointsPerSecond = 10f;

    [Header("Blink effect (works with timeScale=0)")]
    [SerializeField] private float blinkDuration = 3f;
    [SerializeField] private float blinkInterval = 0.25f;

    // Kolor wpisu zrobimy tagami TMP:
    [SerializeField] private Color highlightColor = Color.yellow;

    private float score;
    private bool subscribed;

    private List<int> topScores = new List<int>(5);
    private int lastInsertedIndex = -1;
    private Coroutine blinkRoutine;

    [Serializable]
    private class IntListWrapper
    {
        public List<int> values = new List<int>();
    }

    private void Awake()
    {
        LoadLeaderboard();
        RenderLeaderboardUI(highlightIndex: -1, highlightOn: false);
        UpdateScoreUI(0);
    }

    private void Start()
    {
        StartCoroutine(BindToGameManagerWhenReady());
    }

    private IEnumerator BindToGameManagerWhenReady()
    {
        while (GameManager.Instance == null)
            yield return null;

        if (!subscribed)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            subscribed = true;
        }

        HandleStateChanged(GameManager.Instance.State);
    }

    private void OnDestroy()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        if (subscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            subscribed = false;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        score += pointsPerSecond * Time.deltaTime;
        UpdateScoreUI(Mathf.FloorToInt(score));
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                score = 0f;
                UpdateScoreUI(0);
                break;

            case GameState.GameOver:
                SaveRunToLeaderboard(Mathf.FloorToInt(score));

                // Zawsze wyrenderuj tabelê bez podœwietlenia, a potem ewentualnie migaj
                RenderLeaderboardUI(highlightIndex: -1, highlightOn: false);

                if (blinkRoutine != null)
                    StopCoroutine(blinkRoutine);

                if (lastInsertedIndex >= 0)
                    blinkRoutine = StartCoroutine(BlinkEntryRealtime(lastInsertedIndex));
                break;
        }
    }

    private void SaveRunToLeaderboard(int runScore)
    {
        lastInsertedIndex = -1;

        topScores.Add(runScore);
        topScores = topScores
            .OrderByDescending(x => x)
            .Take(5)
            .ToList();

        // Je¿eli wynik nie wszed³ do Top5, nie bêdzie go w topScores
        // lastInsertedIndex ustawiamy na pierwsze wyst¹pienie (remisy -> najwy¿sze mo¿liwe miejsce)
        for (int i = 0; i < topScores.Count; i++)
        {
            if (topScores[i] == runScore)
            {
                lastInsertedIndex = i;
                break;
            }
        }

        var wrapper = new IntListWrapper { values = topScores };
        string json = JsonUtility.ToJson(wrapper);

        PlayerPrefs.SetString(leaderboardKey, json);
        PlayerPrefs.Save();
    }

    private void LoadLeaderboard()
    {
        topScores.Clear();

        if (!PlayerPrefs.HasKey(leaderboardKey))
            return;

        string json = PlayerPrefs.GetString(leaderboardKey, "");
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var wrapper = JsonUtility.FromJson<IntListWrapper>(json);
            if (wrapper?.values != null)
            {
                topScores = wrapper.values
                    .Where(v => v >= 0)
                    .OrderByDescending(v => v)
                    .Take(5)
                    .ToList();
            }
        }
        catch
        {
            topScores.Clear();
        }
    }

    private void UpdateScoreUI(int s)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {s}";
    }

    private void RenderLeaderboardUI(int highlightIndex, bool highlightOn)
    {
        if (top5Text == null) return;

        string colorHex = ColorUtility.ToHtmlStringRGB(highlightColor);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Top 5:");

        for (int i = 0; i < 5; i++)
        {
            string line;
            if (i < topScores.Count) line = $"{i + 1}) {topScores[i]}";
            else line = $"{i + 1}) —";

            if (i == highlightIndex && highlightOn)
                line = $"<color=#{colorHex}>{line}</color>";

            sb.AppendLine(line);
        }

        top5Text.text = sb.ToString();
    }

    private IEnumerator BlinkEntryRealtime(int index)
    {
        // Uwaga: timeScale=0 -> u¿ywamy realtime, ¿eby miga³o na GameOver
        float elapsed = 0f;
        bool on = false;

        while (elapsed < blinkDuration)
        {
            on = !on;
            RenderLeaderboardUI(highlightIndex: index, highlightOn: on);

            yield return new WaitForSecondsRealtime(blinkInterval);
            elapsed += blinkInterval;
        }

        RenderLeaderboardUI(highlightIndex: -1, highlightOn: false);
        blinkRoutine = null;
    }

    // Opcjonalnie: przycisk w UI
    public void ResetLeaderboard()
    {
        topScores.Clear();
        PlayerPrefs.DeleteKey(leaderboardKey);
        PlayerPrefs.Save();

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        RenderLeaderboardUI(highlightIndex: -1, highlightOn: false);
        score = 0f;
        UpdateScoreUI(0);
    }
}