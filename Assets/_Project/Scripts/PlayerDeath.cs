using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private bool isDead;

    private void Update()
    {
        if (!isDead) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.State == GameState.GameOver &&
            Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.Restart();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.collider.CompareTag("Obstacle"))
        {
            isDead = true;
            Debug.Log("Player died (hit obstacle). Press R to restart.");
            GameManager.Instance?.GameOver();

        }
    }
}