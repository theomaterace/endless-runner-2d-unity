using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private bool isDead;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.collider.CompareTag("Obstacle"))
        {
            isDead = true;

            // SFX
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayHit();

            GameManager.Instance?.GameOver();
        }
    }
}