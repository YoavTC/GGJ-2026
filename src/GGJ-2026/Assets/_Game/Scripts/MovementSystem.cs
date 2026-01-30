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


    public void HandleMovement(bool ground, bool jump, Vector2 move, KnockbackData knokbackData, bool isHeavy)
    {
        /*
        if (knokbackData.Force != 0f)
        {
            // Apply knockback
        }*/

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

        float moveMultiplier = isHeavy ? _heavyMoveMultiplier : 1f;
        float jumpMultiplier = isHeavy ? _heavyJumpMultiplier : 1f;

        // Horizontal movement
        float targetSpeedX = move.x * _moveSpeed * moveMultiplier * bunnyHoppingPunishFactor;


        // Horizontal movement (use movement.x)
       // float targetSpeedX = move.x * _moveSpeed * bunnyHoppingPunishFactor;
        float accel = _acceleration * (ground ? 1f : _airControl);

        float newVelX = Mathf.MoveTowards(
            _rb.linearVelocity.x,
            targetSpeedX,
            accel * Time.deltaTime
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
                _jumpForce * jumpMultiplier * bunnyHoppingPunishFactor
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

    private void Flip()
    {
        _facingRight = !_facingRight;

        // Flip the whole GameObject
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }


}
}
