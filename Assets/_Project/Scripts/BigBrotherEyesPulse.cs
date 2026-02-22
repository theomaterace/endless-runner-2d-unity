using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BigBrotherEyesPulse : MonoBehaviour
{
    [Header("Alpha Pulse")]
    [SerializeField] private float minAlpha = 0.65f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float pulseSpeed = 0.6f;

    [Header("Subtle Drift")]
    [SerializeField] private float driftAmplitudeY = 0.05f;
    [SerializeField] private float driftSpeed = 0.35f;

    private SpriteRenderer sr;
    private Vector3 startPos;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float a = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = sr.color;
        c.a = a;
        sr.color = c;

        float y = Mathf.Sin(Time.time * driftSpeed * Mathf.PI * 2f) * driftAmplitudeY;
        transform.position = new Vector3(startPos.x, startPos.y + y, startPos.z);
    }
}