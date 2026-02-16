using System.Collections.Generic;
using UnityEngine;

public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform segmentsRoot;
    [SerializeField] private Transform spawnPoint;

    [Header("Segments (variants)")]
    [SerializeField] private List<GameObject> segmentPrefabs = new();

    [Header("Spawning")]
    [SerializeField] private int initialSegments = 3;
    [SerializeField] private float spawnLookahead = 2f;

    [Header("Cleanup")]
    [SerializeField] private int maxSegmentsAlive = 6;

    private readonly Queue<GameObject> _aliveSegments = new();
    private Transform _currentEndPoint;
    private Camera _cam;

    // żeby nie losować identycznego segmentu dwa razy pod rząd (jeśli jest wybór)
    private int _lastPrefabIndex = -1;

    private void Start()
    {
        _cam = Camera.main;

        if (!ValidateSetup())
            return;

        SpawnFirstSegment();

        for (int i = 1; i < initialSegments; i++)
            SpawnNextSegment();
    }

    private void Update()
    {
        // jeśli pauza/stop czasu, nie generuj (zwykle i tak wszystko stoi, ale to czytelne zabezpieczenie)
        if (Time.timeScale == 0f) return;

        if (!ValidateRuntime()) return;

        float rightEdgeX = _cam.transform.position.x + _cam.orthographicSize * _cam.aspect;

        if (_currentEndPoint.position.x < rightEdgeX + spawnLookahead)
            SpawnNextSegment();
    }

    private bool ValidateSetup()
    {
        if (segmentsRoot == null)
        {
            Debug.LogError("SegmentSpawner: brak przypisanego Segments Root.");
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("SegmentSpawner: brak przypisanego Spawn Point.");
            return false;
        }

        if (segmentPrefabs == null || segmentPrefabs.Count == 0)
        {
            Debug.LogError("SegmentSpawner: lista segmentPrefabs jest pusta. Dodaj prefaby segmentów w Inspectorze.");
            return false;
        }

        if (segmentPrefabs.Count == 1)
        {
            Debug.LogWarning("SegmentSpawner: na liście jest tylko 1 segment — losowanie nie będzie działać.");
        }

        if (_cam == null)
        {
            Debug.LogError("SegmentSpawner: brak Camera.main w scenie.");
            return false;
        }

        return true;
    }

    private bool ValidateRuntime()
    {
        if (_cam == null || _currentEndPoint == null) return false;
        return true;
    }

    private void SpawnFirstSegment()
    {
        var seg = SpawnRandomSegment();
        if (seg == null) return;

        seg.transform.position = spawnPoint.position;

        _aliveSegments.Enqueue(seg);
        CacheEndPoint(seg);
    }

    private void SpawnNextSegment()
    {
        if (_currentEndPoint == null) return;

        var seg = SpawnRandomSegment();
        if (seg == null) return;

        seg.transform.position = _currentEndPoint.position;

        _aliveSegments.Enqueue(seg);
        CacheEndPoint(seg);

        CleanupIfNeeded();
    }

    private GameObject SpawnRandomSegment()
    {
        int index = GetRandomPrefabIndex();
        var prefab = segmentPrefabs[index];

        if (prefab == null)
        {
            Debug.LogError($"SegmentSpawner: segmentPrefabs[{index}] jest NULL.");
            return null;
        }

        return Instantiate(prefab, segmentsRoot);
    }

    private int GetRandomPrefabIndex()
    {
        int count = segmentPrefabs.Count;

        if (count <= 1)
        {
            _lastPrefabIndex = 0;
            return 0;
        }

        int index = Random.Range(0, count);

        // unikaj powtórki, jeśli jest wybór
        if (index == _lastPrefabIndex)
            index = (index + 1) % count;

        _lastPrefabIndex = index;
        return index;
    }

    private void CacheEndPoint(GameObject seg)
    {
        _currentEndPoint = seg.transform.Find("EndPoint");

        if (_currentEndPoint == null)
            Debug.LogError("Segment prefab nie ma dziecka o nazwie 'EndPoint'!");
    }

    private void CleanupIfNeeded()
    {
        while (_aliveSegments.Count > maxSegmentsAlive)
        {
            var oldest = _aliveSegments.Dequeue();
            if (oldest != null)
                Destroy(oldest);
        }
    }
}