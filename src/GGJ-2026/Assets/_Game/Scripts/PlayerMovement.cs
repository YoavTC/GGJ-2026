using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private PlayerInput _playerInput;

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

    private float _input;
    private bool _wantJump;
    private int jumpCount = 0;

    private void Start()
    {

    }

    void Update()
    {
        // Read player input (use raw for snappier control like in platform fighters)
        _input = _snappyMovement ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");

        if (Input.GetButton("Jump"))
            _wantJump = true;
        else
            jumpCount = 0;

        Debug.Log(Input.GetKeyDown(KeyCode.Joystick2Button0));
    }

    void FixedUpdate()
    {
        bool grounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        float bunnyHoppingPunishFactor = _punishBunnyHopping ? _bunnyHoppingPunishCurve.Evaluate(jumpCount) : 1f;

        // Calculate target and smoothly move towards it. Allow reduced control in air.
        float targetSpeed = _input * _moveSpeed;
        float accel = _acceleration * (grounded ? 1f : _airControl);
        float newVelX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed * bunnyHoppingPunishFactor, accel * Time.fixedDeltaTime);

        if (_wantJump && grounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce * bunnyHoppingPunishFactor);
            jumpCount++;
        }
        _wantJump = false;

        // Apply velocity
        _rb.linearVelocity = new Vector2(newVelX, _rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
