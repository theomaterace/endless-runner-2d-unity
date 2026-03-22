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
    [SerializeField] private TMP_Text scoreText; 
    [SerializeField] private TMP_Text top5Text;  

    [Header("GameOver-only UI")]
    [Tooltip("Assign the Reset button GameObject (or its parent). It will be shown only on GameOver.")]
    [SerializeField] private GameObject resetButtonObject;

    [Header("Scoring")]
    [SerializeField] private float pointsPerSecond = 10f;

    [Header("Blink effect (works with timeScale=0)")]
    [SerializeField] private float blinkDuration = 3f;
    [SerializeField] private float blinkInterval = 0.25f;

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

        SetResetButtonVisible(false);
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
            case GameState.MainMenu:
            case GameState.Paused:
                SetResetButtonVisible(false);
                break;

            case GameState.Playing:
                SetResetButtonVisible(false);

                score = 0f;
                UpdateScoreUI(0);

                break;

            case GameState.GameOver:
                SetResetButtonVisible(true);

                SaveRunToLeaderboard(Mathf.FloorToInt(score));

                RenderLeaderboardUI(highlightIndex: -1, highlightOn: false);

                if (blinkRoutine != null)
                    StopCoroutine(blinkRoutine);

                if (lastInsertedIndex >= 0)
                    blinkRoutine = StartCoroutine(BlinkEntryRealtime(lastInsertedIndex));
                break;
        }
    }

    private void SetResetButtonVisible(bool visible)
    {
        if (resetButtonObject != null)
            resetButtonObject.SetActive(visible);
    }

    private void SaveRunToLeaderboard(int runScore)
    {
        lastInsertedIndex = -1;

        topScores.Add(runScore);
        topScores = topScores
            .OrderByDescending(x => x)
            .Take(5)
            .ToList();

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
            else line = $"{i + 1}) — ";

            if (i == highlightIndex && highlightOn)
                line = $"<color=#{colorHex}>{line}</color>";

            sb.AppendLine(line);
        }

        top5Text.text = sb.ToString();
    }

    private IEnumerator BlinkEntryRealtime(int index)
    {
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