using UnityEngine;

public class ParallaxTiledLayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WorldMover worldMover;
    [SerializeField] private Transform tileA;
    [SerializeField] private Transform tileB;

    [Header("Parallax")]
    [Tooltip("0.1 = bardzo daleko, 0.5 = bliżej, 1.0 = jak świat (raczej nie).")]
    [Range(0f, 1f)]
    [SerializeField] private float speedMultiplier = 0.25f;

    [Tooltip("Jeśli true, skrypt sam ułoży Tile_B na prawo od Tile_A na starcie.")]
    [SerializeField] private bool autoPositionSecondTile = true;

    private Camera cam;
    private float tileWidth;

    private void Awake()
    {
        cam = Camera.main;

        if (tileA == null || tileB == null)
        {
            Debug.LogError($"{nameof(ParallaxTiledLayer)}: Przypnij tileA i tileB w Inspectorze.");
            enabled = false;
            return;
        }

        var sr = tileA.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError($"{nameof(ParallaxTiledLayer)}: tileA musi mieć SpriteRenderer.");
            enabled = false;
            return;
        }

        tileWidth = sr.bounds.size.x;

        if (autoPositionSecondTile)
        {
            tileB.position = new Vector3(tileA.position.x + tileWidth, tileA.position.y, tileA.position.z);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        float worldSpeed = worldMover != null ? worldMover.CurrentSpeed : 5f;
        float layerSpeed = worldSpeed * speedMultiplier;

        Vector3 delta = Vector3.left * layerSpeed * Time.deltaTime;
        tileA.position += delta;
        tileB.position += delta;

        LoopTileIfNeeded(tileA, tileB);
        LoopTileIfNeeded(tileB, tileA);
    }

    private void LoopTileIfNeeded(Transform tile, Transform otherTile)
    {
        if (cam == null) return;

        float halfWidth = tileWidth * 0.5f;
        float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        if (tile.position.x + halfWidth < leftEdge)
        {
            tile.position = new Vector3(otherTile.position.x + tileWidth, tile.position.y, tile.position.z);
        }
    }
}