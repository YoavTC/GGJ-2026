using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private MovementSystem _movementSystem;
    [SerializeField] private CombatSystem _combatSystem;

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

    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext context) => _move = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) => _jump = context.performed;
    public void OnMelee(InputAction.CallbackContext context) => _melee = context.performed;
    public void OnAbility(InputAction.CallbackContext context) => _ability = context.performed;
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

    [SerializeField] private float knockbackPercentage = 0f;

    private float _attackCooldownTimer;

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
        if (_stunTimer > 0)
        {
            Debug.Log("Stunned for " + _stunTimer + " more");
            _stunTimer -= Time.deltaTime;
            return;
        }

        // Ground check
        _ground = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );

        if (_canMove) HandleMovement();
        if (_canAttack) HandleCombat();
    }

    private void HandleMovement()
    {
        _movementSystem.HandleMovement(_ground, _jump, _move, _knockbackData);
        _knockbackData = KnockbackData.Empty;
    }

    private void HandleCombat()
    {
        if (!_canAttack) return;

        if (_ability)
        {
            _canAttack = false;
            _attackCooldownTimer = _abilityCooldown;
            _combatSystem.HandleCombat(melee: false, slamDown: false, ability: true,currentMask.MaskType);
        }
        else if (_melee)
        {
            if (!_ground && _move.y < -0.5f)
            {
                // Slam down attack
                _canAttack = false;
                _attackCooldownTimer = _slamDownCooldown;
                _combatSystem.HandleCombat(melee: false, slamDown: true, ability: false, currentMask.MaskType);
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

    public void HandleGetHit(float damage) 
    {
        Debug.Log("Fuck I'm Hit "+damage);
        knockbackPercentage += damage;
    }
}