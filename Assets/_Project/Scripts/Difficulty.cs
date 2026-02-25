using UnityEngine;

public enum DifficultyLevel
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

[System.Serializable]
public class DifficultySettings
{
    [Header("World speed")]
    public float startSpeed;
    public float acceleration;
    public float maxSpeed;

    [Header("Spawn interval")]
    public float minInterval;
    public float maxInterval;

    [Header("Spawn randomness (0 = sta³y rytm, 1 = pe³na losowoœæ)")]
    [Range(0f, 1f)] public float intervalRandomness;
}

public static class DifficultyStore
{
    private const string Key = "DifficultyLevel";

    public static DifficultyLevel Get()
        => (DifficultyLevel)PlayerPrefs.GetInt(Key, (int)DifficultyLevel.Normal);

    public static void Set(DifficultyLevel level)
    {
        PlayerPrefs.SetInt(Key, (int)level);
        PlayerPrefs.Save();
    }

    public static DifficultySettings GetSettings(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Easy:
                return new DifficultySettings
                {
                    startSpeed = 5.5f,
                    acceleration = 0.12f,
                    maxSpeed = 10.5f,
                    minInterval = 1.3f,
                    maxInterval = 2.2f,
                    intervalRandomness = 0.9f
                };

            case DifficultyLevel.Hard:
                return new DifficultySettings
                {
                    startSpeed = 7.0f,
                    acceleration = 0.28f,
                    maxSpeed = 14.0f,
                    minInterval = 0.75f,
                    maxInterval = 1.35f,
                    intervalRandomness = 0.55f
                };

            default: // Normal
                return new DifficultySettings
                {
                    startSpeed = 6.0f,
                    acceleration = 0.2f,
                    maxSpeed = 12.0f,
                    minInterval = 1.0f,
                    maxInterval = 1.9f,
                    intervalRandomness = 0.75f
                };
        }
    }
}