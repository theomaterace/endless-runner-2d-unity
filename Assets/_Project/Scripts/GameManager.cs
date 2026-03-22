using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    private bool difficultyChosen;

    // Input System actions
    private InputAction pointerPressAction;
    private InputAction pointerPositionAction;
    private InputAction anyKeyAction;
    private InputAction jumpKeyAction;
    private InputAction escapeAction;

    private readonly List<RaycastResult> uiRaycastResults = new();

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
        pointerPressAction?.Enable();
        pointerPositionAction?.Enable();
        anyKeyAction?.Enable();
        jumpKeyAction?.Enable();
        escapeAction?.Enable();
    }

    private void OnDisable()
    {
        pointerPressAction?.Disable();
        pointerPositionAction?.Disable();
        anyKeyAction?.Disable();
        jumpKeyAction?.Disable();
        escapeAction?.Disable();
    }

    private void OnDestroy()
    {
        pointerPressAction?.Dispose();
        pointerPositionAction?.Dispose();
        anyKeyAction?.Dispose();
        jumpKeyAction?.Dispose();
        escapeAction?.Dispose();
    }

    private void CreateInputActions()
    {
        pointerPressAction = new InputAction(
            name: "PointerPress",
            type: InputActionType.Button,
            binding: "<Pointer>/press");

        pointerPositionAction = new InputAction(
            name: "PointerPosition",
            type: InputActionType.Value,
            binding: "<Pointer>/position");

        anyKeyAction = new InputAction(
            name: "AnyKey",
            type: InputActionType.Button,
            binding: "<Keyboard>/anyKey");

        jumpKeyAction = new InputAction(
            name: "JumpKey",
            type: InputActionType.Button);
        jumpKeyAction.AddBinding("<Keyboard>/space");
        jumpKeyAction.AddBinding("<Keyboard>/w");
        jumpKeyAction.AddBinding("<Keyboard>/upArrow");

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

    private bool PointerPressedThisFrame(out Vector2 screenPosition)
    {
        if (pointerPressAction != null && pointerPressAction.WasPressedThisFrame())
        {
            screenPosition = pointerPositionAction != null
                ? pointerPositionAction.ReadValue<Vector2>()
                : default;
            return true;
        }

        screenPosition = default;
        return false;
    }

    private bool PointerOverBlockingUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);

        foreach (var result in uiRaycastResults)
        {
            var go = result.gameObject;
            if (go == null || !go.activeInHierarchy)
                continue;

            var clickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);
            if (clickHandler != null)
                return true;

            var selectable = go.GetComponentInParent<Selectable>();
            if (selectable != null && selectable.IsInteractable())
                return true;
        }

        return false;
    }

    private bool AnyKeyboardKeyDown()
    {
        return anyKeyAction != null && anyKeyAction.WasPressedThisFrame();
    }

    private bool JumpKeyDown()
    {
        return jumpKeyAction != null && jumpKeyAction.WasPressedThisFrame();
    }

    private void Update()
    {
        // Pause
        if (escapeAction != null && escapeAction.WasPressedThisFrame())
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        // Safety
        if (State == GameState.Playing && Time.timeScale < 0.99f)
            Time.timeScale = 1f;
    }
}
