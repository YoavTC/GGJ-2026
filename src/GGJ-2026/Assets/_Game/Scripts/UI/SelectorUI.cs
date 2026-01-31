using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectorUI : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private RectTransform selectorUIPrefab;
    [SerializeField] private RectTransform windowTransform;
    [SerializeField] private RectTransform selector;
    [SerializeField] private List<Image> slotImages;
    [SerializeField] private Sprite emptySlotSprite;

    private Action onMaskSelect;
    private Color color;
    private Sprite selectorSprite;
    private Transform canvasTransform;
    private Transform masksPositionsGridParentTransform;
    private Transform masksGridParentTransform;

    private Queue<MaskScriptableObjext> masks = new Queue<MaskScriptableObjext>(3);

    int _maskCount;
    int _currentIndex;
    float _lastMoveTime = -Mathf.Infinity;
    [SerializeField] private float _moveCooldown = 0.2f;
    private enum Direction
    {
        LEFT, // -1
        RIGHT, // 1
        UP, // -3
        DOWN// 3
    }

    private bool setupFinished = false;

    // Called by CharacterSelectScreen when setting up
    public void Init(
            Action onMaskSelect,
            Color color,
            Sprite selectorSprite,
            Transform canvasTransform,
            Transform masksPositionsGridParentTransform,
            Transform masksGridParentTransform
        )
    {
        this.onMaskSelect = onMaskSelect;
        this.color = color;
        this.selectorSprite = selectorSprite;
        this.canvasTransform = canvasTransform;
        this.masksPositionsGridParentTransform = masksPositionsGridParentTransform;
        this.masksGridParentTransform = masksGridParentTransform;

        setupFinished = false;
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // Clear slots
        foreach (Image slot in slotImages)
        {
            slot.sprite = emptySlotSprite;
        }

        // Setup selector
        Image selectorImg = selector.GetComponent<Image>();
        selectorImg.sprite = selectorSprite;
        selectorImg.color = color;

        // Setup window
        Image windowImg = windowTransform.GetComponent<Image>();
        windowImg.color = color;

        yield return null; // wait one frame to ensure layout is updated
        SetPosition(UnityEngine.Random.Range(0, masksGridParentTransform.childCount - 1));

        setupFinished = true;
    }

    public void OnDeviceLost(PlayerInput evt)
    {
        Debug.Log("Device lost for player input: " + evt);
    }

    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !setupFinished) return;
        Vector2 moveInput = ctx.ReadValue<Vector2>();
        if (moveInput.x < 0) Move(Direction.LEFT);
        else if (moveInput.x > 0) Move(Direction.RIGHT);
        else if (moveInput.y > 0) Move(Direction.UP);
        else if (moveInput.y < 0) Move(Direction.DOWN);
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !setupFinished) return;
        Debug.Log("Submit pressed on index " + _currentIndex);
        onMaskSelect?.Invoke();
        MaskScriptableObjext mask = masksGridParentTransform.GetChild(_currentIndex).GetComponent<MaskUIReference>().mask;

        if (masks.Contains(mask)) return;
        if (masks.Count == 3)
        {
            masks.Dequeue();
        }

        masks.Enqueue(mask);
        UpdateSelectedMasksUI();
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !setupFinished) return;
        Debug.Log("Cancel pressed - clearing masks");
        masks.Clear();
        foreach (Image slot in slotImages)
        {
            slot.sprite = emptySlotSprite;
        }
    }

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
        selector.position = masksPositionsGridParentTransform.GetChild(index).position;
        _currentIndex = index;
    }

    private void UpdateSelectedMasksUI()
    {
        MaskScriptableObjext[] masksArr = masks.ToArray();
        for (int i = 0; i < masksArr.Length; i++)
        {
            slotImages[i].sprite = masksArr[i].MaskIcon;
        }
    }
}