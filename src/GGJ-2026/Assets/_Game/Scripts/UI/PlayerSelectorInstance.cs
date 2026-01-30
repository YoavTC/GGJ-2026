using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelectorInstance : MonoBehaviour
{
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private Transform _masksGridParent;

    int _maskCount;
    int _currentIndex;

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
        _canvasTransform = screen.CanvasTransform;
        _masksGridParent = screen.MasksGridParentTransform;

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
            SetPosition(index);
    }

    private void SetPosition(int index)
    {
        transform.position = _masksGridParent.GetChild(index).position;
        _currentIndex = index;
    }
}
