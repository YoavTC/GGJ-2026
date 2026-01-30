using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [SerializeField] private Transform _meleePoint;
    [SerializeField] private float _meleeDuration;
    [SerializeField] private float _meleeRange;

    [Header("Slam Down Attack Settings")]
    [SerializeField] private Transform _slamDownPoint;
    [SerializeField] private float _slamDownDuration;
    [SerializeField] private float _slamDownRange;

    [Header("Ability Attack Settings")]
    [SerializeField] private Transform _abilityPoint;
    [SerializeField] private float _abilityDuration;
    [SerializeField] private float _abilityRange;

    private List<PlayerController> _hitEnemies = new List<PlayerController>();

    public void OnMelee() => StartCoroutine(Melee());
    private IEnumerator Melee()
    {
        yield return StartCoroutine(Hit(_meleePoint.position, _meleeRange * Vector2.one, _meleeDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
        }
    }

    public void OnSlamDown() => StartCoroutine(SlamDown());
    private IEnumerator SlamDown()
    {
        yield return StartCoroutine(Hit(_slamDownPoint.position, _slamDownRange * Vector2.one, _slamDownDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
        }
    }

    public void OnAbility() => StartCoroutine(Ability());
    private IEnumerator Ability()
    {
        yield return StartCoroutine(Hit(_abilityPoint.position, _abilityRange * Vector2.one, _abilityDuration));
        List<PlayerController> hitEnemies = _hitEnemies;
        foreach (PlayerController enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Apply damage or effects to the enemy here
        }
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
}