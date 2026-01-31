using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectScreen : MonoBehaviour
{
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private Transform _masksPositionsGridParentTransform;
    [SerializeField] private Transform _masksGridParentTransform;
    [SerializeField] private Transform _selectorsGridParentTransform;
    [SerializeField] private PlayerInputManager _playerInputManager;

    [SerializeField] private Color[] _colors;
    [SerializeField] private Sprite[] _selectorIndicatorSprites;

    [SerializeField] private float _selectMaskElementAnimateScale;
    [SerializeField] private float _selectMaskElementAnimateDuration;
    [SerializeField] private Ease _selectMaskElementAnimateEase;

    public void SelectMask(GameObject selectorUI, MaskUIReference maskElement)
    {
        Debug.Log($"Selected mask: {maskElement.mask.MaskName}");
        maskElement.gameObject.transform
            .DOPunchScale(Vector3.one * _selectMaskElementAnimateScale, _selectMaskElementAnimateDuration)
            .SetEase(_selectMaskElementAnimateEase);
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.SetParent(_selectorsGridParentTransform, false);
        SelectorUI selectorUI = playerInput.gameObject.GetComponent<SelectorUI>();
        selectorUI.Init(
            (selectorUI, maskElement) => SelectMask(selectorUI, maskElement),
            _colors[_playerInputManager.playerCount - 1],
            _selectorIndicatorSprites[_playerInputManager.playerCount - 1],
            _canvasTransform,
            _masksPositionsGridParentTransform,
            _masksGridParentTransform);
    }
}
