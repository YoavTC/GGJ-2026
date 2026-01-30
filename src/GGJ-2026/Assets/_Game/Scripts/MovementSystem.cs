using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    private bool _facingRight = true;


    [Header("Settings")]
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
            jumpCount = 0;

        float bunnyHoppingPunishFactor =
            _punishBunnyHopping ? _bunnyHoppingPunishCurve.Evaluate(jumpCount) : 1f;

        // Horizontal movement (use movement.x)
        float targetSpeedX = move.x * _moveSpeed * bunnyHoppingPunishFactor;
        float accel = _acceleration * (ground ? 1f : _airControl);

        float newVelX = Mathf.MoveTowards(
            _rb.linearVelocity.x,
            targetSpeedX,
            accel * Time.fixedDeltaTime
        );

        // Flip sprite based on horizontal movement
        if (newVelX > 0.1f && !_facingRight)
        {
            Flip();
        }
        else if (newVelX < -0.1f && _facingRight)
        {
            Flip();
        }


        // Jump
        if (jump && ground)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _jumpForce * bunnyHoppingPunishFactor
            );
            jumpCount++;
        }

        jump = false;

        // Apply velocity
        _rb.linearVelocity = new Vector2(newVelX, _rb.linearVelocity.y);
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
