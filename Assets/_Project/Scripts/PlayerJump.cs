using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 11f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;

    private Rigidbody2D rb;
    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    // Input System actions
    private InputAction jumpKeyAction;
    private InputAction pointerPressAction;
    private InputAction pointerPositionAction;

    private readonly List<RaycastResult> uiRaycastResults = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        CreateInputActions();
    }

    private void OnEnable()
    {
        jumpKeyAction?.Enable();
        pointerPressAction?.Enable();
        pointerPositionAction?.Enable();
    }

    private void OnDisable()
    {
        jumpKeyAction?.Disable();
        pointerPressAction?.Disable();
        pointerPositionAction?.Disable();
    }

    private void OnDestroy()
    {
        jumpKeyAction?.Dispose();
        pointerPressAction?.Dispose();
        pointerPositionAction?.Dispose();
    }

    private void CreateInputActions()
    {
        jumpKeyAction = new InputAction(
            name: "JumpKeys",
            type: InputActionType.Button);
        jumpKeyAction.AddBinding("<Keyboard>/space");
        jumpKeyAction.AddBinding("<Keyboard>/w");
        jumpKeyAction.AddBinding("<Keyboard>/upArrow");

        pointerPressAction = new InputAction(
            name: "PointerPress",
            type: InputActionType.Button,
            binding: "<Pointer>/press");

        pointerPositionAction = new InputAction(
            name: "PointerPosition",
            type: InputActionType.Value,
            binding: "<Pointer>/position");
    }

    private bool PointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private bool JumpPressedThisFrame()
    {
        if (jumpKeyAction != null && jumpKeyAction.WasPressedThisFrame())
            return true;

        if (pointerPressAction != null && pointerPressAction.WasPressedThisFrame())
        {
            Vector2 screenPosition = pointerPositionAction != null
                ? pointerPositionAction.ReadValue<Vector2>()
                : default;

            return !PointerOverUI(screenPosition);
        }

        return false;
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Nie skacz, je¿eli gra nie jest w Playing
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        if (isGrounded && JumpPressedThisFrame())
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayJump();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
