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


    private List<PlayerController> _hitEnemies = new List<PlayerController>();

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
            enemy.HandleGetHit(_meleeDamage);
        }
        _isAttacking = false;
    }

    private IEnumerator SlamDown()
    {
        _isAttacking = true;
        Debug.Log("Starting Slam Down Attack");
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
            enemy.HandleGetHit(_slamDownDamage);
        }
        _isAttacking = false;
    }

    private IEnumerator Ability(MaskType maskType)
    {
        _isAttacking = false;
        Debug.Log("Starting Ability Attack");
        yield return StartCoroutine(Hit(_abilityPoint.position, _abilityRange * Vector2.one, _abilityDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
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
}