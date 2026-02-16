using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float acceleration = 0.2f;
    [SerializeField] private float maxSpeed = 12f;

    private float currentSpeed;

    private void Start()
    {
        currentSpeed = startSpeed;
    }

    private void Update()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        transform.position += Vector3.left * currentSpeed * Time.deltaTime;
    }
}