using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [SerializeField] private Transform _meleePoint;
    [SerializeField] private float _meleeDuration;
    [SerializeField] private Vector2 _meleeRange;
    [SerializeField] private float _meleeDamage = 3f; // tweak to feel right

    [Header("Slam Down Attack Settings")]
    [SerializeField] private Transform _slamDownPoint;
    [SerializeField] private float _slamDownDuration;
    [SerializeField] private Vector2 _slamDownRange;
    [SerializeField] private float _slamDownForce = 20f; // tweak to feel right
    [SerializeField] private float _slamDownDamage = 10f; // tweak to feel right
    private Rigidbody2D _rb;

    [Header("Ability Attack Settings")]
    [SerializeField] private Transform _abilityPoint;
    [SerializeField] private float _abilityDuration;
    [SerializeField] private Vector2 _abilityRange;

    private bool _isAttacking;
    [SerializeField] private float _deflectDuration = 0.5f;

    [Header("Heavy Mask Settings")]
    [SerializeField] private float _heavyDuration = 2.5f;
    [SerializeField] private float _heavySlamDamageMultiplier = 1.5f;
    [SerializeField] private float _heavyMassMultiplier = 2f;

    private PlayerController _player;
    private float _originalMass;

    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 30f;
    [SerializeField] private float _dashDuration = 0.25f;
    [SerializeField] private float _dashDamage = 30f;




    private List<PlayerController> _hitEnemies = new List<PlayerController>();


    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();

        if (_rb != null)
            _originalMass = _rb.mass;
    }
    public void HandleCombat(bool melee, bool slamDown, bool ability,MaskType maskType)
    {
        if (_isAttacking) return;

        if (ability) StartCoroutine(Ability(maskType));
        else if (slamDown) StartCoroutine(SlamDown());
        else if (melee) StartCoroutine(Melee());
    }

    private IEnumerator Melee()
    {
        _isAttacking = true;
        Debug.Log("Starting Melee Attack");
        yield return StartCoroutine(
            Hit(_meleePoint.position, _meleeRange * Vector2.one, _meleeDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
            enemy.HandleGetHit(_meleeDamage, transform.position, GetComponent<PlayerController>());
        }
        _isAttacking = false;
    }

    private IEnumerator SlamDown()
    {
        _isAttacking = true;
        Debug.Log("Starting Slam Down Attack");
        float slamDamage = _slamDownDamage;

        if (_player != null && _player.IsHeavy)
            slamDamage *= _heavySlamDamageMultiplier;
        _rb = GetComponent<Rigidbody2D>();

        if (_rb != null)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -_slamDownForce);
        }

        yield return StartCoroutine(Hit(_slamDownPoint.position, _slamDownRange * Vector2.one, _slamDownDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
            enemy.HandleGetHit(slamDamage, transform.position, _player);
        }
        _isAttacking = false;
    }

    private IEnumerator Ability(MaskType maskType)
    {
        _isAttacking = false;
        Debug.Log("Starting Ability Attack");

        yield return StartCoroutine(Hit(_abilityPoint.position, _abilityRange * Vector2.one, _abilityDuration));
        /*
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
        }*/
        switch (maskType) 
        { 
            case MaskType.ANCHOR:
                    break;
            case MaskType.DEFLECT:
                Deflect();
                break;
            case MaskType.HEAVY:
                Heavy();
                break;
            case MaskType.KAMIKAZE:
                break;
            case MaskType.DASH:
                yield return StartCoroutine(Dash());
                break;
        }
        _isAttacking = false;
    }

    // Get all enemies in range during the melee duration
    private IEnumerator Hit(Vector2 pos, Vector2 size, float duration)
    {
        _hitEnemies.Clear();
        List<PlayerController> hitEnemies = new List<PlayerController>();
        float timer = 0f;
        while (timer <= duration)
        {
            Collider2D[] hit = Physics2D.OverlapBoxAll(pos, size, 0);
            if (hit.Length > 0)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].gameObject == this.gameObject) continue;
                    if (hit[i].TryGetComponent<PlayerController>(out PlayerController enemy))
                    {
                        if (!hitEnemies.Contains(enemy))
                        {
                            hitEnemies.Add(enemy);
                        }
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        _hitEnemies = hitEnemies;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_meleePoint.position, _meleeRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_slamDownPoint.position, _slamDownRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_abilityPoint.position, _abilityRange);
    }
    //Mask Abilities:
    private void Deflect()
    {
        StartCoroutine(DeflectCoroutine());
    }

    private IEnumerator DeflectCoroutine()
    {
        PlayerController player = GetComponent<PlayerController>();
        if (player == null) yield break;

        Debug.Log("DEFLECT ACTIVE");
        player.SetDeflecting(true);

        yield return new WaitForSeconds(_deflectDuration);

        player.SetDeflecting(false);
        Debug.Log("DEFLECT ENDED");
    }

    private void Heavy()
    {
        StartCoroutine(HeavyCoroutine());
    }

    private IEnumerator HeavyCoroutine()
    {
        if (_player == null || _rb == null) yield break;

        Debug.Log("HEAVY MASK ACTIVE");

        _player.SetHeavy(true);
        _rb.mass = _originalMass * _heavyMassMultiplier;

        yield return new WaitForSeconds(_heavyDuration);

        _rb.mass = _originalMass;
        _player.SetHeavy(false);

        Debug.Log("HEAVY MASK ENDED");
    }

    private IEnumerator Dash()
    {
        _isAttacking = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        PlayerController player = GetComponent<PlayerController>();

        if (rb == null || player == null)
            yield break;

        player.SetDashing(true);

        // Save original physics state
        float originalGravity = rb.gravityScale;
        Vector2 originalVelocity = rb.linearVelocity;

        // Lock movement and prepare dash
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // Determine dash direction (facing)
        Vector2 dashDir = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

        // Rotate player sideways
        transform.rotation = Quaternion.Euler(0, 0, -90f * dashDir.x);

        // Apply dash velocity directly
        rb.linearVelocity = dashDir * _dashSpeed;

        float timer = 0f;
        HashSet<PlayerController> hitPlayers = new HashSet<PlayerController>();

        while (timer < _dashDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Restore physics state
        rb.gravityScale = originalGravity;

        // Smoothly damp dash velocity to avoid floaty linger
        rb.linearVelocity *= 0.2f;

        transform.rotation = Quaternion.identity;
        player.SetDashing(false);

        _isAttacking = false;
    }


}



