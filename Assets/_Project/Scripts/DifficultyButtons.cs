using UnityEngine;

public class DifficultyButtons : MonoBehaviour
{
    [Header("Root object to show/hide (panel with buttons)")]
    [SerializeField] private GameObject buttonsRoot;

    private void Reset()
    {
        buttonsRoot = gameObject;
    }

    public void Show()
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(true);
    }

    public void Hide()
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(false);
    }

    private void Chosen()
    {
        Hide();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyDifficultyChosen();
            GameManager.Instance.StartGame();
        }
    }

    public void SetEasy()
    {
        DifficultyStore.Set(DifficultyLevel.Easy);
        Chosen();
    }

    public void SetNormal()
    {
        DifficultyStore.Set(DifficultyLevel.Normal);
        Chosen();
    }

    public void SetHard()
    {
        DifficultyStore.Set(DifficultyLevel.Hard);
        Chosen();
    }
}