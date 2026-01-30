using UnityEngine;

public class RopeSwingSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;

    [Header("Physics Settings")]
    [SerializeField] private float _gravity = 60f;
    [SerializeField] private float _swingDamping = 0.05f;
    [SerializeField] private float _playerSwingPower = 25f;
    [SerializeField] private float _ropeTension = 2f;
    [SerializeField] private float _maxSwingVelocity = 50f;

    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 1f;

    [Header("Release")]
    [SerializeField] private float _releaseBoost = 1.2f;

    [Header("Debug - Press E to release")]
    [SerializeField] private KeyCode _debugReleaseKey = KeyCode.E;

    private RopeSwing _currentRope;
    private bool _isSwinging;
    private Vector3 _anchorPoint;
    private float _ropeLength;
    private float _originalGravityScale;
    private float _originalDrag;
    private Vector2 _swingVelocity;

    private void Start()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        
        _originalGravityScale = _rb.gravityScale;
        _originalDrag = _rb.linearDamping;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_debugReleaseKey) && _isSwinging)
        {
            ReleaseRope();
        }
    }

    public void TryLatchToRope()
    {
        if (_isSwinging) return;

        RopeSwing[] allRopes = FindObjectsByType<RopeSwing>(FindObjectsSortMode.None);
        
        RopeSwing nearestRope = null;
        float nearestDistance = float.MaxValue;
        Vector3 nearestPoint = Vector3.zero;

        foreach (RopeSwing rope in allRopes)
        {
            Vector3 anchorPos = rope.GetAnchorPosition();
            float ropeLength = rope.GetRopeLength();
            
            Vector3 ropeBottom = anchorPos + Vector3.down * ropeLength;
            Vector3 closestPoint = GetClosestPointOnLine(anchorPos, ropeBottom, transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);

            if (distance < _detectionRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRope = rope;
                nearestPoint = closestPoint;
            }
        }

        if (nearestRope != null)
        {
            LatchToRope(nearestRope, nearestPoint);
        }
    }

    private Vector3 GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    private void LatchToRope(RopeSwing rope, Vector3 latchPoint)
    {
        _currentRope = rope;
        _isSwinging = true;
        _anchorPoint = rope.GetAnchorPosition();
        _ropeLength = Vector3.Distance(_anchorPoint, latchPoint);

        _currentRope.SetAttachedPlayer(transform);

        _swingVelocity = _rb.linearVelocity;
        
        _rb.gravityScale = 0f;
        _rb.linearDamping = 0f;
    }

    public void ReleaseRope()
    {
        if (!_isSwinging) return;

        if (_currentRope != null)
        {
            _currentRope.SetAttachedPlayer(null);
        }

        _rb.linearVelocity = _swingVelocity * _releaseBoost;
        _rb.gravityScale = _originalGravityScale;
        _rb.linearDamping = _originalDrag;

        _isSwinging = false;
        _currentRope = null;
        _swingVelocity = Vector2.zero;
    }

    public void HandleSwing()
    {
        if (!_isSwinging) return;

        Vector2 currentPos = transform.position;
        Vector2 toPlayer = currentPos - (Vector2)_anchorPoint;
        float currentDistance = toPlayer.magnitude;
        Vector2 ropeDir = toPlayer.normalized;
        
        Vector2 gravityForce = Vector2.down * _gravity;
        _swingVelocity += gravityForce * Time.fixedDeltaTime;
        
        Vector2 tangent = new Vector2(-ropeDir.y, ropeDir.x);
        float tangentialSpeed = Vector2.Dot(_swingVelocity, tangent);
        _swingVelocity = tangent * tangentialSpeed;
        
        _swingVelocity -= _swingVelocity * _swingDamping;
        
        if (currentDistance > _ropeLength)
        {
            Vector2 tensionForce = -ropeDir * (currentDistance - _ropeLength) * _ropeTension;
            _swingVelocity += tensionForce * Time.fixedDeltaTime;
            
            Vector2 constrainedPos = (Vector2)_anchorPoint + ropeDir * _ropeLength;
            _rb.position = constrainedPos;
            currentPos = constrainedPos;
        }
        
        float speed = _swingVelocity.magnitude;
        if (speed > _maxSwingVelocity)
        {
            _swingVelocity = _swingVelocity.normalized * _maxSwingVelocity;
        }
        
        _rb.linearVelocity = _swingVelocity;
    }

    public void ApplyPlayerControl(float horizontalInput)
    {
        if (!_isSwinging || Mathf.Approximately(horizontalInput, 0f)) return;

        Vector2 ropeDir = ((Vector2)_anchorPoint - (Vector2)transform.position).normalized;
        Vector2 perpendicular = new Vector2(ropeDir.y, -ropeDir.x);
        
        Vector2 controlForce = perpendicular * horizontalInput * _playerSwingPower;
        _swingVelocity += controlForce * Time.fixedDeltaTime;
    }

    public bool IsSwinging()
    {
        return _isSwinging;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        if (_isSwinging)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_anchorPoint, transform.position);
            Gizmos.DrawWireSphere(_anchorPoint, 0.3f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + _swingVelocity.normalized * 2f);
        }
        else
        {
            RopeSwing[] allRopes = FindObjectsByType<RopeSwing>(FindObjectsSortMode.None);
            
            foreach (RopeSwing rope in allRopes)
            {
                Vector3 anchorPos = rope.GetAnchorPosition();
                float ropeLength = rope.GetRopeLength();
                Vector3 ropeBottom = anchorPos + Vector3.down * ropeLength;
                Vector3 closestPoint = GetClosestPointOnLine(anchorPos, ropeBottom, transform.position);
                float distance = Vector3.Distance(transform.position, closestPoint);

                if (distance < _detectionRadius)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, closestPoint);
                    Gizmos.DrawWireSphere(closestPoint, 0.2f);
                }
            }
        }
    }
}
