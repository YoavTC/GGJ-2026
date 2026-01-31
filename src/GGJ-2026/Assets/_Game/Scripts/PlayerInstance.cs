using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstance : MonoBehaviour
{
    [SerializeField] private SelectorUI _selectorUIPrefab;
    [SerializeField] private PlayerController _playerPrefab;
    public SelectorUI SelectorUI => _selectorUIPrefab;

    [SerializeField] PlayerInput _playerInput;

    private void Awake()
    {
        // Change input to handle the mask selection UI
        //_playerInput.defaultActionMap = ;
        _playerInput.SwitchCurrentActionMap("UI");
    }
}
