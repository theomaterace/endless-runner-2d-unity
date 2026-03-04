using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float deathShakeDuration = 0.3f;
    [SerializeField] private float deathShakeMagnitude = 0.4f;

    private bool isDead;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.collider.CompareTag("Obstacle"))
        {
            isDead = true;

            // Efekt dźwiękowy
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayHit();

            // Wstrząs kamery
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);

            // Logika końca gry
            GameManager.Instance?.GameOver();
        }
    }
}