using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;

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

    public void HandleMovement(bool ground, bool jump, Vector2 move, KnockbackData knokbackData)
    {
        if (knokbackData.Force != 0f)
        {
            // Apply knockback
        }

        if (ground)
        {
            jumpCount = 0;
        }
        else if (jumpCount == 0) 
        {
            // If we fall off a ledge without jumping, count it as the first jump used
            jumpCount = 1;
        }

        float bunnyHoppingPunishFactor =
            _punishBunnyHopping ? _bunnyHoppingPunishCurve.Evaluate(jumpCount) : 1f;

        // Horizontal movement (use movement.x)
        float targetSpeedX = move.x * _moveSpeed * bunnyHoppingPunishFactor;
        float accel = _acceleration * (ground ? 1f : _airControl);

        float newVelX = Mathf.MoveTowards(
            _rb.linearVelocity.x,
            targetSpeedX,
            accel * Time.deltaTime
        );

        int maxJumps = _doubleJump ? 2 : 1;
        if (jump && jumpCount < maxJumps)
        {
            _rb.linearVelocity = new Vector2(
                newVelX,
                _jumpForce * bunnyHoppingPunishFactor
            );
            jumpCount++;
        }
        else
        {
            // Apply velocity
            _rb.linearVelocity = new Vector2(newVelX, _rb.linearVelocity.y);
        }
    }
}