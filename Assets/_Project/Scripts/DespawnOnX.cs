using UnityEngine;

public class DespawnOnX : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;

    private void Update()
    {
        if (transform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }
}