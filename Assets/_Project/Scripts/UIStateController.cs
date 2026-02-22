using System.Collections;
using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    private bool _subscribed;

    private void Awake()
    {
        // Bezpieczny start: ukryj to, co ma byæ ukryte.
        // (Opcjonalnie – mo¿esz to usun¹æ, jeœli wolisz ustawiaæ rêcznie w Hierarchy)
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Start()
    {
        // Start mo¿e odpaliæ siê zanim GameManager zd¹¿y ustawiæ Instance (zale¿nie od kolejnoœci w hierarchii).
        // Dlatego robimy retry przez coroutine.
        StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        // Poczekaj a¿ singleton bêdzie gotowy
        while (GameManager.Instance == null)
            yield return null;

        // Pod³¹cz event tylko raz
        if (!_subscribed)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            _subscribed = true;
        }

        // Zsynchronizuj UI ze stanem od razu
        HandleStateChanged(GameManager.Instance.State);
    }

    private void OnDestroy()
    {
        if (_subscribed && GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            _subscribed = false;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(state == GameState.MainMenu);
        if (hudPanel != null) hudPanel.SetActive(state == GameState.Playing);
        if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);
    }

    // ---- Button hooks (opcjonalne) ----

    public void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void GoToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMenu();
    }

    public void Restart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Restart();
    }
}