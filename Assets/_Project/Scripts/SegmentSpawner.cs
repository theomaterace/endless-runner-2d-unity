using System.Collections.Generic;
using UnityEngine;

public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform segmentsRoot;
    [SerializeField] private Transform spawnPoint;

    [Header("Segments")]
    [SerializeField] private GameObject segmentPrefab;

    [Header("Spawning")]
    [SerializeField] private int initialSegments = 3;
    [SerializeField] private float spawnLookahead = 2f;

    [Header("Cleanup")]
    [SerializeField] private int maxSegmentsAlive = 6;

    private readonly Queue<GameObject> _aliveSegments = new();
    private Transform _currentEndPoint;
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;

        SpawnFirstSegment();
        for (int i = 1; i < initialSegments; i++)
            SpawnNextSegment();
    }

    private void Update()
    {
        if (_cam == null || _currentEndPoint == null) return;

        float rightEdgeX = _cam.transform.position.x + _cam.orthographicSize * _cam.aspect;

        if (_currentEndPoint.position.x < rightEdgeX + spawnLookahead)
            SpawnNextSegment();
    }

    private void SpawnFirstSegment()
    {
        var seg = Instantiate(segmentPrefab, segmentsRoot);
        seg.transform.position = spawnPoint.position;

        _aliveSegments.Enqueue(seg);
        CacheEndPoint(seg);
    }

    private void SpawnNextSegment()
    {
        if (_currentEndPoint == null) return;

        var seg = Instantiate(segmentPrefab, segmentsRoot);
        seg.transform.position = _currentEndPoint.position;

        _aliveSegments.Enqueue(seg);
        CacheEndPoint(seg);

        CleanupIfNeeded();
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