using UnityEngine;

public class StopRunInAir : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerJump playerJump;

    [Header("Animator Speeds")]
    [SerializeField] private float groundedSpeed = 1f;
    [SerializeField] private float airSpeed = 0f; 

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (playerJump == null) playerJump = GetComponentInParent<PlayerJump>();
    }

    private void Update()
    {
        if (animator == null || playerJump == null) return;

        animator.speed = playerJump.IsGrounded ? groundedSpeed : airSpeed;
    }
}