using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

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

    [Header("Input")]
    [Tooltip("Jump keys should NOT start the game, so the first Space doesn't get 'eaten' by Start.")]
    [SerializeField] private bool preventJumpKeysFromStarting = true;

    private bool difficultyChosen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (pressAnyKeyStartText != null)
            pressAnyKeyStartText.gameObject.SetActive(State == GameState.MainMenu);

        if (pressAnyKeyRestartText != null)
            pressAnyKeyRestartText.gameObject.SetActive(State == GameState.GameOver);

        if (difficultyButtons != null)
        {
            if (State == GameState.MainMenu) difficultyButtons.Show();
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

    private bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsMouseClickDown()
    {
        return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
    }

    private bool AnyKeyboardKeyDown()
    {
        if (!Input.anyKeyDown) return false;
        if (IsMouseClickDown()) return false; // odfiltruj mysz
        return true;
    }

    private bool JumpKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.UpArrow);
    }

    private void Update()
    {
        // MAIN MENU
        if (State == GameState.MainMenu)
        {
            if (requireDifficultyChoice && !difficultyChosen)
                return;

            // Start z klawiatury
            if (AnyKeyboardKeyDown())
            {
                if (preventJumpKeysFromStarting && JumpKeyDown())
                {
                    // Nie startujemy na Space/W/Up, ¿eby nie by³o wra¿enia "opóŸnienia skoku"
                    return;
                }

                StartGame();
                return;
            }

            // Start klikniêciem poza UI (opcjonalnie)
            if (Input.GetMouseButtonDown(0) && !PointerOverUI())
            {
                StartGame();
                return;
            }
        }

        // GAME OVER: restart klawiatur¹ lub klikniêciem
        if (State == GameState.GameOver)
        {
            if (AnyKeyboardKeyDown() || Input.GetMouseButtonDown(0))
            {
                Restart();
                return;
            }
        }

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        // Safety
        if (State == GameState.Playing && Time.timeScale < 0.99f)
            Time.timeScale = 1f;
    }
}