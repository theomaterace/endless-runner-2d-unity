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
    [Tooltip("If enabled, W/Up won't start the game. Space WILL start (so user can start with jump).")]
    [SerializeField] private bool preventJumpKeysFromStarting = true; // CHANGED tooltip meaning

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
        bool canShowStartPrompt = (State == GameState.MainMenu) && (!requireDifficultyChoice || difficultyChosen);

        if (pressAnyKeyStartText != null)
            pressAnyKeyStartText.gameObject.SetActive(canShowStartPrompt);

        if (pressAnyKeyRestartText != null)
            pressAnyKeyRestartText.gameObject.SetActive(State == GameState.GameOver);

        if (difficultyButtons != null)
        {
            bool shouldShowDifficulty =
                (State == GameState.MainMenu) &&
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

    // CHANGED: rozdzielam Space od W/Up, ¿eby Space móg³ startowaæ
    private bool DisallowedStartKeyDown()
    {
        // Nie blokujemy Space
        return Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.UpArrow);
    }

    private void Update()
    {
        // MAIN MENU
        if (State == GameState.MainMenu)
        {
            if (requireDifficultyChoice && !difficultyChosen)
                return;

            // 1) Space startuje (i mo¿esz to zrobiæ nawet zanim anyKeyDown "przepuœci" inne rzeczy)
            if (Input.GetKeyDown(KeyCode.Space)) // CHANGED
            {
                StartGame();
                return;
            }

            // 2) Start z klawiatury (dowolny klawisz)
            if (AnyKeyboardKeyDown())
            {
                if (preventJumpKeysFromStarting && DisallowedStartKeyDown()) // CHANGED
                {
                    // Nie startujemy na W/Up (opcjonalnie), ale Space ju¿ obs³u¿yliœmy wy¿ej
                    return;
                }

                StartGame();
                return;
            }

            // 3) Start klikniêciem poza UI
            if (Input.GetMouseButtonDown(0) && !PointerOverUI())
            {
                StartGame();
                return;
            }
        }

        // GAME OVER: restart klawiatur¹ lub klikniêciem POZA UI
        if (State == GameState.GameOver)
        {
            // klawiatura -> restart
            if (AnyKeyboardKeyDown())
            {
                Restart();
                return;
            }

            // mysz -> restart tylko jeœli nie klikamy UI (np. ResetButton)
            if (Input.GetMouseButtonDown(0))
            {
                if (PointerOverUI())
                    return;

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