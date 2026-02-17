using UnityEngine;

public class PlayerVisualAnimator : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Run Bob")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobFrequency = 10f;

    [Header("Air Tilt")]
    [SerializeField] private float maxTiltDeg = 12f;
    [SerializeField] private float tiltSmooth = 10f;

    [Header("Squash & Stretch")]
    [SerializeField] private float squashAmount = 0.08f;
    [SerializeField] private float squashReturn = 14f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.12f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 baseLocalPos;
    private Vector3 baseLocalScale;
    private float bobTime;
    private bool wasGrounded;

    private void Reset()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, baseLocalPos, Time.unscaledDeltaTime * 12f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.unscaledDeltaTime * 12f);
            transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.unscaledDeltaTime * squashReturn);
            return;
        }

        if (rb == null)
            return;

        bool grounded = IsGrounded();

        if (grounded)
        {
            bobTime += Time.deltaTime * bobFrequency;
            float bob = Mathf.Sin(bobTime) * bobAmplitude;
            transform.localPosition = baseLocalPos + new Vector3(0f, bob, 0f);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, baseLocalPos, Time.deltaTime * 12f);
        }

        float vy = rb.linearVelocity.y;
        float targetTilt = grounded ? 0f : Mathf.Clamp(-vy * 2.0f, -maxTiltDeg, maxTiltDeg);

        float currentZ = transform.localEulerAngles.z;
        if (currentZ > 180f)
            currentZ -= 360f;

        float newZ = Mathf.Lerp(currentZ, targetTilt, Time.deltaTime * tiltSmooth);
        transform.localRotation = Quaternion.Euler(0f, 0f, newZ);

        if (!wasGrounded && grounded)
        {
            KickScale(1f + squashAmount, 1f - squashAmount);
        }
        else if (wasGrounded && !grounded && vy > 0.1f)
        {
            KickScale(1f - squashAmount, 1f + squashAmount);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, baseLocalScale, Time.deltaTime * squashReturn);

        wasGrounded = grounded;
    }

    private void KickScale(float xMul, float yMul)
    {
        transform.localScale = new Vector3(
            baseLocalScale.x * xMul,
            baseLocalScale.y * yMul,
            baseLocalScale.z
        );
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask) != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
