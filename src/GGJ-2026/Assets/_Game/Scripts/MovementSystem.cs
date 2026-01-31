using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    private bool _facingRight = true;


    [Header("Settings")]
    [SerializeField] private bool _doubleJump = true;
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _airControl = 0.6f;
    [SerializeField] private float _jumpForce = 12f;
    [Space]
    [SerializeField] private bool _punishBunnyHopping = false;
    [SerializeField] private AnimationCurve _bunnyHoppingPunishCurve;
    private int jumpCount;
    [Header("Heavy Mask Multipliers")]
    [SerializeField] private float _heavyMoveMultiplier = 0.6f;
    [SerializeField] private float _heavyJumpMultiplier = 0.7f;

    [SerializeField]
    private Animator _animator;


    private bool _hasLeftGround = false; // track if player is in air

    private bool _isInAir = false; // tracks if the player has left the ground

    public void HandleMovement(bool ground, bool jump, Vector2 move, KnockbackData knokbackData, bool isHeavy)
    {
        if (GetComponent<PlayerController>().IsDashing)
            return;

        // Reset jump count and air flag when grounded
        if (ground)
        {
            jumpCount = 0;
            _isInAir = false;
        }
        else if (jumpCount == 0)
        {
            // walking off ledge counts as first jump
            jumpCount = 1;
        }

        float bunnyHoppingPunishFactor = _punishBunnyHopping ? _bunnyHoppingPunishCurve.Evaluate(jumpCount) : 1f;
        float moveMultiplier = isHeavy ? _heavyMoveMultiplier : 1f;
        float jumpMultiplier = isHeavy ? _heavyJumpMultiplier : 1f;

        // Horizontal movement
        float targetSpeedX = move.x * _moveSpeed * moveMultiplier * bunnyHoppingPunishFactor;
        float accel = _acceleration * (ground ? 1f : _airControl);

        float newVelX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeedX, accel * Time.deltaTime);

        // Flip sprite
        if (newVelX > 0.1f && !_facingRight) Flip();
        else if (newVelX < -0.1f && _facingRight) Flip();

        int maxJumps = _doubleJump ? 2 : 1;

        // Jumping
        if (jump && jumpCount < maxJumps)
        {
            _rb.linearVelocity = new Vector2(newVelX, _jumpForce * jumpMultiplier * bunnyHoppingPunishFactor);
            jumpCount++;
            _isInAir = true; // player left the ground
        }
        else
        {
            _rb.linearVelocity = new Vector2(newVelX, _rb.linearVelocity.y);

            if (!ground && !_isInAir && Mathf.Abs(_rb.linearVelocity.y) > 0f)
            {
                // player walked off a ledge
                _isInAir = true;
            }
        }

        // --- Animator updates ---
        // Walking only when grounded and moving horizontally
        _animator.SetBool("isWalking", ground && Mathf.Abs(newVelX) > 0.1f);

        // Jumping any time player is in air
        _animator.SetBool("isJumping", _isInAir);
    }




    private void Flip()
    {
        _facingRight = !_facingRight;

        // Flip the whole GameObject
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }
}