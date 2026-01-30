using UnityEngine;

public class RopeSwing : MonoBehaviour
{
    [Header("Rope Properties")]
    [SerializeField] private Transform _ropeTopAnchor;
    [SerializeField] private float _ropeLength = 5f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Visual")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private int _segmentCount = 20;
    [SerializeField] private float _ropeCurve = 0.5f;
    [SerializeField] private float _swaySpeed = 2f;
    [SerializeField] private float _swayAmount = 0.2f;

    private Transform _attachedPlayer;
    private Transform _attachmentPoint;
    private float _swayTime;
    private Vector3 _restPosition;

    private void Start()
    {
        if (_ropeTopAnchor == null)
        {
            _ropeTopAnchor = transform;
        }

        _restPosition = _ropeTopAnchor.position + Vector3.down * _ropeLength;
        SetupVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void SetupVisual()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        _lineRenderer.positionCount = _segmentCount;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.08f;
        _lineRenderer.useWorldSpace = true;
    }

    private void UpdateVisual()
    {
        if (_lineRenderer == null) return;

        Vector3 topPosition = _ropeTopAnchor.position;
        Vector3 endPosition;

        if (_attachedPlayer != null)
        {
            endPosition = _attachmentPoint != null ? _attachmentPoint.position : _attachedPlayer.position;
            _restPosition = Vector3.Lerp(_restPosition, endPosition, Time.deltaTime * 10f);
        }
        else
        {
            _swayTime += Time.deltaTime * _swaySpeed;
            
            float swayX = Mathf.Sin(_swayTime) * _swayAmount;
            endPosition = topPosition + Vector3.down * _ropeLength + Vector3.right * swayX;
            
            _restPosition = Vector3.Lerp(_restPosition, endPosition, Time.deltaTime * 3f);
        }

        Vector3 direction = (_restPosition - topPosition);
        float distance = direction.magnitude;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward).normalized;

        for (int i = 0; i < _segmentCount; i++)
        {
            float t = i / (float)(_segmentCount - 1);
            Vector3 position = Vector3.Lerp(topPosition, _restPosition, t);
            
            float curve = Mathf.Sin(t * Mathf.PI) * _ropeCurve;
            position += perpendicular * curve;
            
            _lineRenderer.SetPosition(i, position);
        }
    }

    public void SetAttachedPlayer(Transform player)
    {
        _attachedPlayer = player;
        
        if (player != null)
        {
            Transform handPoint = player.Find("RopeAttachPoint");
            _attachmentPoint = handPoint != null ? handPoint : player;
        }
        else
        {
            _attachmentPoint = null;
        }
    }

    public Vector3 GetAnchorPosition()
    {
        return _ropeTopAnchor.position;
    }

    public float GetRopeLength()
    {
        return _ropeLength;
    }

    private void OnDrawGizmos()
    {
        if (_ropeTopAnchor == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_ropeTopAnchor.position, 0.2f);
        
        Vector3 bottomPosition = _ropeTopAnchor.position + Vector3.down * _ropeLength;
        Gizmos.DrawLine(_ropeTopAnchor.position, bottomPosition);
        Gizmos.DrawWireSphere(bottomPosition, 0.15f);
    }
}
