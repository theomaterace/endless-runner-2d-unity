using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform parentWorld;

    [Header("Spawn timing (seconds)")]
    [SerializeField] private float minInterval = 1.0f;
    [SerializeField] private float maxInterval = 2.0f;

    [Header("Spawn position")]
    [SerializeField] private float spawnY = -2.5f;

    [Header("Obstacle size")]
    [SerializeField] private float minHeight = 1.0f;
    [SerializeField] private float maxHeight = 2.5f;

    private float timer;
    private float currentInterval;

    private void Start()
    {
        currentInterval = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        if (obstaclePrefab == null || parentWorld == null) return;

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            currentInterval = Random.Range(minInterval, maxInterval);
            Spawn();
        }
    }

    private void Spawn()
    {
        Vector3 pos = new Vector3(transform.position.x, spawnY, 0f);
        GameObject obj = Instantiate(obstaclePrefab, pos, Quaternion.identity, parentWorld);

        float h = Random.Range(minHeight, maxHeight);
        Vector3 s = obj.transform.localScale;
        obj.transform.localScale = new Vector3(s.x, h, s.z);
    }
}