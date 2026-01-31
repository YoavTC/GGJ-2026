using System.Collections;
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
    [SerializeField] private Camera _mainCamera;

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

        // Check if should start
        if (_playerInputManager.playerCount > 1 && allPlayersReady())
        {
            StopAllCoroutines();
            StartCoroutine(StartGame());
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        DontDestroyOnLoad(playerInput.gameObject);
        SelectorUI selectorUI = playerInput.gameObject.GetComponent<PlayerInstance>().SelectorUI;
        Debug.Log(selectorUI);
        selectorUI.transform.SetParent(_selectorsGridParentTransform, false);
        Debug.Log(selectorUI);
        selectorUI.Init(
            (selectorUI, maskElement) => SelectMask(selectorUI, maskElement),
            _colors[_playerInputManager.playerCount - 1],
            _selectorIndicatorSprites[_playerInputManager.playerCount - 1],
            _canvasTransform,
            _masksPositionsGridParentTransform,
            _masksGridParentTransform);
    }

    public IEnumerator StartGame()
    {
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"Starting game in {5 - i}...");
            yield return new WaitForSeconds(1);
            if (!allPlayersReady())
            {
                Debug.Log("A player unselected a mask, cancelling game start.");
                yield break;
            }
        }

        //Destroy(_mainCamera.gameObject);
        Bootstrap.Instance.ChangeState(GameState.IN_GAME);
    }

    private bool allPlayersReady()
    {
        SelectorUI[] selectors = _selectorsGridParentTransform.GetComponentsInChildren<SelectorUI>();
        Debug.Log($"Number of selectors: {selectors.Length}");
        foreach (SelectorUI selector in selectors)
        {
            Debug.Log(selector);
            if (!selector.hasSelectedMasks)
            {
                Debug.Log("A player has not selected masks yet.");
                return false;
            }
        }

        Debug.Log("All players have selected masks.");
        return true;
    }
}
