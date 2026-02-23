using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Start in MainMenu (game paused) until you start it.
    public GameState State { get; private set; } = GameState.MainMenu;

    public event Action<GameState> OnStateChanged;

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text pressAnyKeyStartText;
    [SerializeField] private TMP_Text pressAnyKeyRestartText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate detected -> destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[GameManager] Awake -> Instance set: " + gameObject.name);
    }

    private void Start()
    {
        Debug.Log("[GameManager] Start -> initial State=" + State);
        ApplyState(State);
        OnStateChanged?.Invoke(State);
        RefreshPromptUI();
        Debug.Log("[GameManager] Start -> timeScale=" + Time.timeScale);
    }

    public void SetState(GameState newState)
    {
        Debug.Log($"[GameManager] SetState called: {State} -> {newState}");

        if (State == newState) return;

        State = newState;
        ApplyState(State);
        OnStateChanged?.Invoke(State);
        RefreshPromptUI();

        Debug.Log("[GameManager] SetState done -> State=" + State + ", timeScale=" + Time.timeScale);
    }

    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
            case GameState.GameOver:
            case GameState.MainMenu:
                Time.timeScale = 0f;
                break;

            default:
                // Safety: if you ever add new states, don't accidentally freeze the game.
                Time.timeScale = 1f;
                break;
        }

        Debug.Log("[GameManager] ApplyState -> " + state + ", timeScale=" + Time.timeScale);
    }

    private void RefreshPromptUI()
    {
        if (pressAnyKeyStartText != null)
            pressAnyKeyStartText.gameObject.SetActive(State == GameState.MainMenu);

        if (pressAnyKeyRestartText != null)
            pressAnyKeyRestartText.gameObject.SetActive(State == GameState.GameOver);
    }

    // Public API (UI + other scripts)
    public void StartGame() => SetState(GameState.Playing);
    public void GoToMenu() => SetState(GameState.MainMenu);

    public void Pause()
    {
        if (State == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void GameOver() => SetState(GameState.GameOver);

    public void Restart()
    {
        Debug.Log("[GameManager] Restart -> reloading scene");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        // VERY IMPORTANT: in Unity, keyboard input is captured only if the Game view is focused.
        // Click the Game tab once, then press a key.

        // START: any key (instead of only Space)
        if (State == GameState.MainMenu && Input.anyKeyDown)
        {
            Debug.Log("[GameManager] Any key pressed -> StartGame (State=" + State + ")");
            StartGame();
            return;
        }

        // RESTART: any key on GameOver (optional, keeps your R too)
        if (State == GameState.GameOver && Input.anyKeyDown)
        {
            Debug.Log("[GameManager] Any key pressed -> Restart (State=" + State + ")");
            Restart();
            return;
        }

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[GameManager] Escape pressed (State=" + State + ")");
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        // Keep R as fallback (optional)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[GameManager] R pressed (State=" + State + ")");
            if (State == GameState.GameOver) Restart();
        }

        // Safety: if something else keeps forcing timeScale to 0, this will reveal it immediately.
        if (State == GameState.Playing && Time.timeScale < 0.99f)
        {
            Debug.LogWarning("[GameManager] timeScale was not 1 during Playing. Forcing to 1. (timeScale=" + Time.timeScale + ")");
            Time.timeScale = 1f;
        }
    }
}