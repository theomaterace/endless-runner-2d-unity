using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources (assign)")]
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioSource gameplayMusic;

    [Header("Optional")]
    [SerializeField] private bool keepAcrossSceneLoads = false;

    private GameState lastState = (GameState)(-1);

    private void Awake()
    {
        if (keepAcrossSceneLoads)
            DontDestroyOnLoad(gameObject);

        if (menuMusic != null) menuMusic.Stop();
        if (gameplayMusic != null) gameplayMusic.Stop();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.State);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == lastState) return;
        lastState = state;

        bool shouldPlayMenu =
            state == GameState.MainMenu ||
            state == GameState.GameOver ||
            state == GameState.Paused;

        bool shouldPlayGameplay = state == GameState.Playing;

        if (menuMusic != null)
        {
            if (shouldPlayMenu)
            {
                if (!menuMusic.isPlaying) menuMusic.Play();
            }
            else
            {
                if (menuMusic.isPlaying) menuMusic.Stop();
            }
        }

        if (gameplayMusic != null)
        {
            if (shouldPlayGameplay)
            {
                if (!gameplayMusic.isPlaying) gameplayMusic.Play();
            }
            else
            {
                if (gameplayMusic.isPlaying) gameplayMusic.Stop();
            }
        }
    }
}
