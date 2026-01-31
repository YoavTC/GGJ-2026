using DigitalRuby.LightningBolt;
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

    [Header("Kamikaze Settings")]
    [SerializeField] private float _kamikazeDamage = 40f;
    [SerializeField] private float _kamikazeSelfDamageMin = 0.5f;
    [SerializeField] private float _kamikazeSelfDamageMax = 0.75f;
    [SerializeField] private Vector2 _kamikazeRange = new Vector2(3f, 3f);
    [SerializeField] private float _kamikazeChargeTime = 0.75f;
    [Header("Kamikaze Glow")]
    [SerializeField] private Color _kamikazeGlowColor = Color.red;
    [SerializeField] private float _kamikazeGlowIntensity = 4f;
    [SerializeField] private float _kamikazeGlowPulseSpeed = 20f;

    [SerializeField]
    private AnimationCurve _kamikazeChargeCurve =
        AnimationCurve.EaseInOut(0, 1, 1, 1);
    private SpriteRenderer _sprite;
    private SkinnedMeshRenderer _meshRenderer;
    private Material[] _materials;
    private Color[] _originalEmissionColors;

    [Header("Stun Mask Settings")]
    [SerializeField] private float _stunRange = 5f;             // How far the beam goes
    [SerializeField] private float _stunWidth = 1f;             // Width of the hitbox line
    [SerializeField] private float _stunDuration = 1.5f;        // Duration targets are stunned
    [SerializeField] private float _stunCooldown = 0.5f;        // Optional small delay for attack duration
    [SerializeField] private float _stunDamage = 0f;            // Optional damage if you want
    [Header("Stun Beam Visual")]
    [SerializeField] private GameObject _stunBeamPrefab; // prefab with LineRenderer
    [SerializeField] private float _stunBeamDuration = 0.15f; // beam lasts briefly
    [SerializeField] private float _stunBeamWidth = 0.15f;
    [Header("Stun Beam Settings")]
    [SerializeField] private Transform _stunBeamOrigin; // assign in inspector





    private List<PlayerController> _hitEnemies = new List<PlayerController>();


    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        if (_rb != null)
            _originalMass = _rb.mass;

        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (_meshRenderer != null)
        {
            _materials = _meshRenderer.materials;
            _originalEmissionColors = new Color[_materials.Length];

            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i].HasProperty("_EmissionColor"))
                {
                    _originalEmissionColors[i] =
                        _materials[i].GetColor("_EmissionColor");

                    _materials[i].EnableKeyword("_EMISSION");
                }
            }
        }
    }
    public void HandleCombat(bool melee, bool slamDown, bool ability, MaskType maskType, Vector2 aimDir = default)

    {
        if (_isAttacking) return;

        if (ability) StartCoroutine(Ability(maskType, aimDir));
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

    private IEnumerator Ability(MaskType maskType,Vector2 inputDirection)
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
                Kamikaze();
                break;
            case MaskType.DASH:
               //yield return StartCoroutine(Dash());
                break;
            case MaskType.STUN:
                Stun(inputDirection); // You’ll pass aim direction here
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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, _kamikazeRange);
        
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + Vector2.up * (_stunRange / 2f); // placeholder
        Gizmos.DrawWireCube(center, new Vector2(_stunWidth, _stunRange));


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

    private IEnumerator Dash(Vector2 direction, bool isGrounded)
    {
        _isAttacking = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        PlayerController player = GetComponent<PlayerController>();
        if (rb == null || player == null)
            yield break;

        player.SetDashing(true);

        // Save physics
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        // Normalize input, fallback to facing if zero
        if (direction.sqrMagnitude < 0.01f)
            direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);

           direction.Normalize();

        // Clamp downward dash if on ground
        if (isGrounded && direction.y < 0f)
            direction.y = 0f;

        float timer = 0f;

        while (timer < _dashDuration)
        {
            timer += Time.deltaTime;

            // Apply dash velocity
            rb.linearVelocity = direction * _dashSpeed;

            // Compute Z-axis rotation only
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Debug.Log("Rotation Dash Angle" + angle);

            //transform.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // Restore physics
        rb.gravityScale = originalGravity;
        rb.linearVelocity *= 0.2f;
        transform.rotation = Quaternion.identity;
        player.SetDashing(false);

        _isAttacking = false;
    }





    public void HandleDash(Vector2 inputDir, bool isGrounded)
    {
        if (_isAttacking) return;
        StartCoroutine(Dash(inputDir, isGrounded));
    }

    private void Kamikaze()
    {
        StartCoroutine(KamikazeCoroutine());
    }

    private IEnumerator KamikazeCoroutine()
    {
        _isAttacking = true;

        Debug.Log("KAMIKAZE CHARGING");

        PlayerController player = GetComponent<PlayerController>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (player == null || rb == null)
            yield break;

        // ===== LOCK MOVEMENT =====
        rb.linearVelocity = Vector2.zero;

        float timer = 0f;

        // ===== CHARGE UP =====
        while (timer < _kamikazeChargeTime)
        {
            timer += Time.deltaTime;
            float t = timer / _kamikazeChargeTime;

            // PULSING EMISSION GLOW
            if (_materials != null)
            {
                float pulse =
                    0.5f + Mathf.Sin(Time.time * _kamikazeGlowPulseSpeed) * 0.5f;

                float intensity =
                    Mathf.Lerp(0f, _kamikazeGlowIntensity, t) * pulse;

                for (int i = 0; i < _materials.Length; i++)
                {
                    if (_materials[i].HasProperty("_EmissionColor"))
                    {
                        _materials[i].SetColor(
                            "_EmissionColor",
                            _kamikazeGlowColor * intensity
                        );
                    }
                }
            }

            yield return null;
        }

        // ===== BLOOM POP (1 FRAME) =====
        if (_materials != null)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i].HasProperty("_EmissionColor"))
                {
                    _materials[i].SetColor(
                        "_EmissionColor",
                        _kamikazeGlowColor * (_kamikazeGlowIntensity * 2f)
                    );
                }
            }
        }

        // Let bloom catch this frame
        yield return null;

        Debug.Log("KAMIKAZE EXPLODE");

        // ===== EXPLOSION =====
        Collider2D[] hit = Physics2D.OverlapBoxAll(
            transform.position,
            _kamikazeRange,
            0f
        );

        foreach (Collider2D col in hit)
        {
            if (!col.TryGetComponent<PlayerController>(out PlayerController target))
                continue;

            if (target == player)
            {
                float selfMultiplier = Random.Range(
                    _kamikazeSelfDamageMin,
                    _kamikazeSelfDamageMax
                );

                float selfDamage = _kamikazeDamage * selfMultiplier;

                // Bias knockback upwards with some randomness
                Vector2 randomOffset = new Vector2(
                    Random.Range(-0.6f, 0.6f),   // horizontal variance
                    Random.Range(1.5f, 2.5f)    // strong upward bias
                );

                // Fake hit origin BELOW the player so force pushes up
                Vector2 fakeHitOrigin =
                    (Vector2)transform.position - randomOffset;

                target.HandleGetHit(
                    selfDamage,
                    fakeHitOrigin,
                    player
                );

            }
            else
            {
                target.HandleGetHit(_kamikazeDamage, transform.position, player);
            }
        }

        // ===== RESET MATERIALS =====
        if (_materials != null)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i].HasProperty("_EmissionColor"))
                {
                    _materials[i].SetColor(
                        "_EmissionColor",
                        _originalEmissionColors[i]
                    );
                }
            }
        }

        _isAttacking = false;
    }



    private void Stun(Vector2 direction)
    {
        StartCoroutine(StunCoroutine(direction));
    }

    private IEnumerator StunCoroutine(Vector2 direction)
    {
        _isAttacking = true;

        PlayerController player = GetComponent<PlayerController>();
        if (player == null) yield break;

        // Normalize direction
        if (direction.sqrMagnitude < 0.01f)
            direction = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
        direction.Normalize();

        // Start and end points
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(direction * _stunRange);

        // === Spawn Visual Beam ===
        GameObject beam = new GameObject("StunBeam");
        LineRenderer lr = beam.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = _stunBeamWidth;
        lr.endWidth = _stunBeamWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.cyan;
        lr.endColor = Color.cyan;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        beam.transform.parent = transform; // optional

        // === Stun Hitbox ===
        float beamLength = Vector2.Distance(start, end);

        // Center of box = midpoint
        Vector2 center = (Vector2)start + direction * (beamLength / 2f);
        // Size of box: width = stun width, height = beam length
        Vector2 size = new Vector2(_stunWidth, beamLength);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // rotate because OverlapBox treats y as height

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);
        foreach (Collider2D col in hits)
        {
            if (col.gameObject == this.gameObject) continue;

            if (col.TryGetComponent<PlayerController>(out PlayerController target))
            {
                target.HandleStun(_stunDuration);
                if (_stunDamage > 0f)
                    target.HandleGetHit(_stunDamage, transform.position, player);
            }
        }

        yield return new WaitForSeconds(_stunBeamDuration);

        Destroy(beam);
        _isAttacking = false;
    }









}



