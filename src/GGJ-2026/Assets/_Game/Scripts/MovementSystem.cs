using UnityEngine;
using UnityEngine.InputSystem;

public class MovementSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    public InputAction playerMovement;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _airControl = 0.6f;
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _groundCheckRadius = 0.1f;

    [Space]
    [SerializeField] private bool _snappyMovement = true;
    [SerializeField] private bool _punishBunnyHopping = false;
    [SerializeField] private AnimationCurve _bunnyHoppingPunishCurve;

    private Vector2 movement;
    private bool _wantJump;
    private int jumpCount;

    private void OnEnable()
    {
        playerMovement.Enable();
    }

    private void OnDisable()
    {
        playerMovement.Disable();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            _wantJump = true;
    }

    private void HandleMovement()
    {
        bool grounded = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );

        if (grounded)
            jumpCount = 0;

        float bunnyHoppingPunishFactor =
            _punishBunnyHopping ? _bunnyHoppingPunishCurve.Evaluate(jumpCount) : 1f;

        // Horizontal movement (use movement.x)
        float targetSpeedX = movement.x * _moveSpeed * bunnyHoppingPunishFactor;
        float accel = _acceleration * (grounded ? 1f : _airControl);

        float newVelX = Mathf.MoveTowards(
            _rb.linearVelocity.x,
            targetSpeedX,
            accel * Time.fixedDeltaTime
        );

        // Jump
        if (_wantJump && grounded)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _jumpForce * bunnyHoppingPunishFactor
            );
            jumpCount++;
        }

        _wantJump = false;

        // Apply velocity
        _rb.linearVelocity = new Vector2(newVelX, _rb.linearVelocity.y);
    }
}
