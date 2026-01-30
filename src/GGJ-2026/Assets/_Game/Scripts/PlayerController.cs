using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private MovementSystem _movementSystem;
    [SerializeField] private CombatSystem _combatSystem;
    [SerializeField] private RopeSwingSystem _ropeSwingSystem;

    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    public List<MaskScriptableObjext> playerMasks = new List<MaskScriptableObjext>(3);
    private MaskScriptableObjext currentMask;

    // Input states
    Vector2 _move;
    bool _jump;
    bool _melee;
    bool _ability;
    bool _ropeGrab;

    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext context) => _move = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) => _jump = context.performed;
    public void OnMelee(InputAction.CallbackContext context) => _melee = context.performed;
    public void OnAbility(InputAction.CallbackContext context) => _ability = context.performed;
    public void OnRopeGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _ropeGrab = true;
        }
    }
    #endregion

    // State variables
    private float _stunTimer;
    private bool _isAlive;
    private bool _canMove;
    private bool _canAttack;
    private bool _ground;
    private KnockbackData _knockbackData;

    // Health variables
    private int _livesRemaining;
    private float _knockbackMultiplier;

    [Header("Combat Cooldowns")]
    [SerializeField] private float _meleeCooldown = 0.5f;
    [SerializeField] private float _slamDownCooldown = 1.0f;
    [SerializeField] private float _abilityCooldown = 2.0f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForceMultiplier = 0.2f;
    [SerializeField] private float minKnockbackAngle = 10f;
    [SerializeField] private float maxKnockbackAngle = 25f;
    [SerializeField] private float knockbackStunTime = 0.15f;
    [SerializeField] private float knockbackPercentage = 0f;

    private float _attackCooldownTimer;

    [Header("Deflect")]
    public bool IsDeflecting { get; private set; }
    [SerializeField] private GameObject deflectBubble;

    [Header("Heavy")]
    public bool IsHeavy { get; private set; }
    public float KnockbackResistanceMultiplier => IsHeavy ? 0.5f : 1f;
    //private Dictionary<MaskType, float> _maskCooldownTimers;
    private float _maskCooldownTimer;

    [Header("Dash")]
    [SerializeField] private float dashDamage = 30f;

    private bool _isDashing;

    public bool IsDashing => _isDashing;
    public void SetDashing(bool value) => _isDashing = value;



    private void Start()
    {
        _isAlive = true;
        _canMove = true;
        _canAttack = true;
        _livesRemaining = 3;
        _knockbackMultiplier = 1f;
        if (playerMasks.Count > 0) { currentMask = playerMasks[0]; }
    }

    private void Update()
    {
        if (!_isAlive)
        {
            if (_livesRemaining == 0)
            {
                Debug.Log("Kill player..");
                // Remove component
                return;
            }

            // Respawn player
            Debug.Log("Respawn player..");
            return;
        }

        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer <= 0f)
            {
                _canAttack = true;
            }
        }

        // Cooldown stun timer
        if (_stunTimer > 0f)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer < 0f)
                _stunTimer = 0f;
        }

        if (_maskCooldownTimer > 0f)
        {
            _maskCooldownTimer -= Time.deltaTime;
            if (_maskCooldownTimer < 0f)
                _maskCooldownTimer = 0f;
        }

        // Ground check
        _ground = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );

        if (_canMove && _stunTimer <= 0f) HandleMovement();
        if (_canAttack && _stunTimer <= 0f) HandleCombat();
        HandleRopeSwing();
    }

    private void FixedUpdate()
    {
        if (_ropeSwingSystem != null && _ropeSwingSystem.IsSwinging())
        {
            _ropeSwingSystem.HandleSwing();
        }
    }

    private void HandleMovement()
    {
        if (_ropeSwingSystem != null && _ropeSwingSystem.IsSwinging())
        {
            return;
        }
       
        _movementSystem.HandleMovement(_ground, _jump, _move, _knockbackData, IsHeavy);
        _knockbackData = KnockbackData.Empty;
        _jump = false;
    }

    private void HandleRopeSwing()
    {
        if (_ropeSwingSystem == null) return;

        if (_ropeSwingSystem.IsSwinging())
        {
            // Jump to release when already swinging
            if (_jump)
            {
                _ropeSwingSystem.ReleaseRope();
                _jump = false;
            }
            
            _ropeSwingSystem.ApplyPlayerControl(_move.x);
        }
        else
        {
            // Rope grab to latch when not swinging
            if (_ropeGrab)
            {
                _ropeSwingSystem.TryLatchToRope();
                _ropeGrab = false;
            }
        }
    }

    private void HandleCombat()
    {
        if (!_canAttack) return;

        if (_ability)
        {
            if (_maskCooldownTimer > 0f)
                return;

            _canAttack = false;
            _attackCooldownTimer = currentMask.maskCooldown;
            _maskCooldownTimer = currentMask.maskCooldown;

            if (currentMask.MaskType == MaskType.DASH)
            {
                // Only dash downwards if in air
                Vector2 dashInput = _move;
                if (_ground && dashInput.y < -0.1f)
                    dashInput.y = 0f; // block downward dash on ground

                _combatSystem.HandleDash(dashInput.normalized, _ground);
            }
            else
            {
                _combatSystem.HandleCombat(false, false, true, currentMask.MaskType);
            }
        }
        else if (_melee)
        {
            if (!_ground && _move.y < -0.5f)
            {
                // Slam down attack
                _canAttack = false;
                _attackCooldownTimer = _slamDownCooldown;
                _combatSystem.HandleCombat(melee: false, slamDown: true, ability: false, currentMask.MaskType);
                _stunTimer = 0.5f;
            }
            else
            {
                // Normal melee
                _canAttack = false;
                _attackCooldownTimer = _meleeCooldown;
                _combatSystem.HandleCombat(melee: true, slamDown: false, ability: false, currentMask.MaskType);
            }
        }

    }

    public void HandleGetHit(float damage,Vector2 attackerPosition,PlayerController attacker=null) 
    {
        if (IsDeflecting && attacker != null)
        {
            Debug.Log($"{name} DEFLECTED the attack!");

            attacker.HandleGetHit(damage, transform.position);
            return;
        }

        Debug.Log("Fuck I'm Hit "+damage);
        knockbackPercentage += damage;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Determine horizontal direction away from attacker
        float horizontalDir = transform.position.x < attackerPosition.x ? -1f : 1f;

        // Random upward angle
        float upAngle = Random.Range(minKnockbackAngle, maxKnockbackAngle) * Mathf.Deg2Rad;
        /*
        if (_ground)
            angle = Mathf.Max(angle, 5f);
        */
        // Convert angle to direction
        Vector2 direction = new Vector2(
        horizontalDir * Mathf.Cos(upAngle),
         Mathf.Sin(upAngle)
            );
        direction.y = Mathf.Abs(direction.y); // ensure upward

        // Calculate final force
        //float force = Mathf.Pow(knockbackPercentage + damage, 1.1f) * knockbackForceMultiplier;
        float force =Mathf.Pow(knockbackPercentage + damage, 1.15f)* knockbackForceMultiplier * KnockbackResistanceMultiplier;
        Debug.DrawRay(transform.position, direction * 3f, Color.red, 1f);

        // Apply knockback
        rb.linearVelocity = Vector2.zero; // reset existing velocity
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        // Brief stun
        _stunTimer = knockbackStunTime;
    }

    public void SetDeflecting(bool value)
    {
        IsDeflecting = value;
        if (IsDeflecting) { deflectBubble.SetActive(true); }else{ deflectBubble.SetActive(false); }
    }

    public void SetHeavy(bool value)
    {
        IsHeavy = value;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isDashing) return;

        if (collision.gameObject.TryGetComponent<PlayerController>(out var enemy))
        {
            enemy.HandleGetHit(30f, transform.position);
        }
    }


}