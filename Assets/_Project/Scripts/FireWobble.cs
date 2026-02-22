using UnityEngine;

public class FireWobble : MonoBehaviour
{
    [Header("Position wobble")]
    [SerializeField] private float posAmplitudeY = 0.03f;
    [SerializeField] private float posSpeed = 8f;

    [Header("Scale flicker")]
    [SerializeField] private float scaleAmplitudeX = 0.06f;
    [SerializeField] private float scaleAmplitudeY = 0.10f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Rotation (optional)")]
    [SerializeField] private float rotAmplitude = 2.5f;
    [SerializeField] private float rotSpeed = 7f;

    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private float phase1, phase2, phase3;

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;

        phase1 = Random.Range(0f, 100f);
        phase2 = Random.Range(0f, 100f);
        phase3 = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float t = Time.time;

        float flickerY = 1f + Mathf.Sin((t + phase3) * scaleSpeed) * scaleAmplitudeY;
        float flickerX = 1f + Mathf.Sin((t + phase2) * scaleSpeed) * scaleAmplitudeX;

        Vector3 newScale = new Vector3(
            baseLocalScale.x * flickerX,
            baseLocalScale.y * flickerY,
            baseLocalScale.z
        );

        transform.localScale = newScale;

        // Kompensacja, żeby dół został w miejscu
        float heightDifference = (newScale.y - baseLocalScale.y);
        transform.localPosition = baseLocalPos + new Vector3(0f, heightDifference * 0.5f, 0f);
    }
}