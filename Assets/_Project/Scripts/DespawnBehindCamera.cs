using UnityEngine;

public class DespawnBehindCamera : MonoBehaviour
{
    [SerializeField] private float extraBehind = 3f;

    private void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        if (transform.position.x < leftEdge - extraBehind)
        {
            Destroy(gameObject);
        }
    }
}
