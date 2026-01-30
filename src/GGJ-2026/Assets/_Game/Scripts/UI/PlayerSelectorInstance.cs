using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSelectorInstance : MonoBehaviour
{
    private Transform _canvasTransform;
    private Transform _masksGridParent;

    [SerializeField] private float _moveCooldown = 0.2f; // cooldown in seconds

    [SerializeField] private Image _selectorImage;

    int _maskCount;
    int _currentIndex;

    private float _lastMoveTime = -Mathf.Infinity;

    private enum Direction
    {
        LEFT, // -1
        RIGHT, // 1
        UP, // -3
        DOWN// 3
    }

    private void Awake()
    {
        CharacterSelectScreen screen = FindFirstObjectByType<CharacterSelectScreen>();
        (Transform, Transform, Color, Sprite) references = screen.GetReferences();
        _canvasTransform = references.Item1;
        _masksGridParent = references.Item2;
        _selectorImage.color = references.Item3;
        _selectorImage.sprite = references.Item4;

        transform.SetParent(_canvasTransform);
        _maskCount = _masksGridParent.childCount - 1;
        SetPosition(Random.Range(0, _maskCount));
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        if (moveInput.x < 0) Move(Direction.LEFT);
        else if (moveInput.x > 0) Move(Direction.RIGHT);
        else if (moveInput.y > 0) Move(Direction.UP);
        else if (moveInput.y < 0) Move(Direction.DOWN);
    }

    public void OnMoveLeft(InputAction.CallbackContext ctx) { if (ctx.performed) Move(Direction.LEFT); }
    public void OnMoveRight(InputAction.CallbackContext ctx) { if (ctx.performed) Move(Direction.RIGHT); }
    public void OnMoveUp(InputAction.CallbackContext ctx) { if (ctx.performed) Move(Direction.UP); }
    public void OnMoveDown(InputAction.CallbackContext ctx) { if (ctx.performed) Move(Direction.DOWN); }

    private void Move(Direction direction)
    {
        // enforce cooldown
        if (Time.time - _lastMoveTime < _moveCooldown)
            return;

        int index = _currentIndex;

        switch (direction)
        {
            case Direction.LEFT:
                // can't move left from left column (0 and 3)
                if (index != 0 && index != 3)
                    index -= 1;
                break;
            case Direction.RIGHT:
                // can't move right from right column (2 and 5)
                if (index != 2 && index != 5)
                    index += 1;
                break;
            case Direction.UP:
                // can only move up if currently on bottom row (index >= 3)
                if (index >= 3)
                    index -= 3;
                break;
            case Direction.DOWN:
                // can only move down if currently on top row (index <= 2)
                if (index <= 2)
                    index += 3;
                break;
        }

        // move only if index changed
        if (index != _currentIndex)
        {
            SetPosition(index);
            _lastMoveTime = Time.time;
        }
    }

    private void SetPosition(int index)
    {
        transform.position = _masksGridParent.GetChild(index).position;
        _currentIndex = index;
    }
}
