using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager _playerInputManager;

    void Start()
    {
        // Optionally, you can set up player joining logic here
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput input)
    {
        Debug.Log($"Player {input.playerIndex} joined the game.");

    }
}
