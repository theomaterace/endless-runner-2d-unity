using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.MainMenu;

    public event Action<GameState> OnStateChanged;

    [Header("UI (assign your existing objects)")]
    [SerializeField] private TMP_Text pressAnyKeyStartText;
    [SerializeField] private TMP_Text pressAnyKeyRestartText;
    [SerializeField] private DifficultyButtons difficultyButtons;

    [Header("Flow")]
    [SerializeField] private bool requireDifficultyChoice = true;

    private bool difficultyChosen;
    private InputAction escapeAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateInputActions();
    }

    private void OnEnable()
    {
        escapeAction?.Enable();
    }

    private void OnDisable()
    {
        escapeAction?.Disable();
    }

    private void OnDestroy()
    {
        escapeAction?.Dispose();
    }

    private void CreateInputActions()
    {
        escapeAction = new InputAction(
            name: "Escape",
            type: InputActionType.Button,
            binding: "<Keyboard>/escape");
    }

    private void Start()
    {
        difficultyChosen = !requireDifficultyChoice;

        ApplyState(State);
        OnStateChanged?.Invoke(State);
        RefreshPromptUI();
    }

    public void NotifyDifficultyChosen()
    {
        difficultyChosen = true;
        RefreshPromptUI();
    }

    public void SetState(GameState newState)
    {
        if (State == newState) return;

        State = newState;
        ApplyState(State);
        OnStateChanged?.Invoke(State);
        RefreshPromptUI();
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
                Time.timeScale = 1f;
                break;
        }
    }

    private void RefreshPromptUI()
    {
        bool canShowStartPrompt = State == GameState.MainMenu && (!requireDifficultyChoice || difficultyChosen);

        if (pressAnyKeyStartText != null)
            pressAnyKeyStartText.gameObject.SetActive(canShowStartPrompt);

        if (pressAnyKeyRestartText != null)
            pressAnyKeyRestartText.gameObject.SetActive(State == GameState.GameOver);

        if (difficultyButtons != null)
        {
            bool shouldShowDifficulty =
                State == GameState.MainMenu &&
                requireDifficultyChoice &&
                !difficultyChosen;

            if (shouldShowDifficulty) difficultyButtons.Show();
            else difficultyButtons.Hide();
        }
    }

    public void StartGame() => SetState(GameState.Playing);
    public void GoToMenu() => SetState(GameState.MainMenu);
    public void Pause() { if (State == GameState.Playing) SetState(GameState.Paused); }
    public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }
    public void GameOver() => SetState(GameState.GameOver);

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (escapeAction != null && escapeAction.WasPressedThisFrame())
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        if (State == GameState.Playing && Time.timeScale < 0.99f)
            Time.timeScale = 1f;
    }
}
