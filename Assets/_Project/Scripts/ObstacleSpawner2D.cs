using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner2D : MonoBehaviour
{
    [Header("World parent (must be the moving World)")]
    [SerializeField] private Transform parentWorld;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> obstaclePrefabs = new();

    [Header("Spawn timing (seconds)")]
    [SerializeField] private float minInterval = 1.0f;
    [SerializeField] private float maxInterval = 2.0f;

    [Header("Spawn randomness (0 = sta³y rytm, 1 = pe³na losowoœæ)")]
    [SerializeField, Range(0f, 1f)] private float intervalRandomness = 0.75f;

    [Header("Spawn position")]
    [SerializeField] private float spawnAhead = 2.0f;
    [SerializeField] private float spawnY = -2.0f;

    [Header("Optional random height (scales Y)")]
    [SerializeField] private bool randomizeHeight = false;
    [SerializeField] private float minHeightScale = 0.8f;
    [SerializeField] private float maxHeightScale = 1.6f;

    private float timer;
    private float nextInterval;
    private int lastIndex = -1;

    private void Start()
    {
        ApplyDifficulty();
        nextInterval = GetNextInterval();
    }

    private void Update()
    {
        if (parentWorld == null) return;
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= nextInterval)
        {
            timer = 0f;
            nextInterval = GetNextInterval();
            Spawn();
        }
    }

    private void ApplyDifficulty()
    {
        var level = DifficultyStore.Get();
        var settings = DifficultyStore.GetSettings(level);

        minInterval = settings.minInterval;
        maxInterval = settings.maxInterval;
        intervalRandomness = settings.intervalRandomness;
    }

    private float GetNextInterval()
    {
        float r = Random.value;

        // im mniejsza losowoœæ, tym bli¿ej œrodka zakresu
        r = Mathf.Lerp(0.5f, r, intervalRandomness);

        return Mathf.Lerp(minInterval, maxInterval, r);
    }

    private void Spawn()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float rightEdge = cam.transform.position.x + cam.orthographicSize * cam.aspect;
        float spawnX = rightEdge + spawnAhead;

        int index = Random.Range(0, obstaclePrefabs.Count);
        if (obstaclePrefabs.Count > 1 && index == lastIndex)
            index = (index + 1) % obstaclePrefabs.Count;

        lastIndex = index;

        GameObject obj = Instantiate(
            obstaclePrefabs[index],
            new Vector3(spawnX, spawnY, 0f),
            Quaternion.identity,
            parentWorld
        );

        if (randomizeHeight)
        {
            float h = Random.Range(minHeightScale, maxHeightScale);
            Vector3 s = obj.transform.localScale;
            obj.transform.localScale = new Vector3(s.x, h, s.z);
        }
    }
}