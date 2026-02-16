using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;

    public event Action<GameState> OnStateChanged;

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
        ApplyState(State);
    }

    public void SetState(GameState newState)
    {
        if (State == newState) return;

        State = newState;
        ApplyState(State);
        OnStateChanged?.Invoke(State);
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
        }
    }

    public void StartGame() => SetState(GameState.Playing);
    public void Pause() => SetState(GameState.Paused);
    public void Resume() => SetState(GameState.Playing);
    public void GameOver() => SetState(GameState.GameOver);

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing)
                Pause();
            else if (State == GameState.Paused)
                Resume();
        }
    }
}