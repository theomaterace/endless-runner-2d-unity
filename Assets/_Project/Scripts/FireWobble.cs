using UnityEngine;

public class FireWobble : MonoBehaviour
{
    [Header("Scale flicker")]
    [SerializeField] private float scaleAmplitudeX = 0.06f;
    [SerializeField] private float scaleAmplitudeY = 0.10f;
    [SerializeField] private float scaleSpeed = 12f;

    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private float phaseX;
    private float phaseY;

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;

        phaseX = Random.Range(0f, 100f);
        phaseY = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float t = Time.time;

        float flickerY = 1f + Mathf.Sin((t + phaseY) * scaleSpeed) * scaleAmplitudeY;
        float flickerX = 1f + Mathf.Sin((t + phaseX) * scaleSpeed) * scaleAmplitudeX;

        Vector3 newScale = new Vector3(
            baseLocalScale.x * flickerX,
            baseLocalScale.y * flickerY,
            baseLocalScale.z
        );

        transform.localScale = newScale;

        float heightDifference = newScale.y - baseLocalScale.y;
        transform.localPosition = baseLocalPos + new Vector3(0f, heightDifference * 0.5f, 0f);
    }
}
