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

    public void SelectMask(GameObject mask)
    {
        Debug.Log($"Selected mask: {mask.name}");
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.SetParent(_selectorsGridParentTransform, false);
        SelectorUI selectorUI = playerInput.gameObject.GetComponent<SelectorUI>();
        selectorUI.Init(
            () => SelectMask(playerInput.gameObject),
            _colors[_playerInputManager.playerCount - 1],
            _selectorIndicatorSprites[_playerInputManager.playerCount - 1],
            _canvasTransform,
            _masksPositionsGridParentTransform,
            _masksGridParentTransform);
    }
}
