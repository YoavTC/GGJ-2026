using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerJoinScript : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private List<Transform> spawnPoints;

    public UnityEvent NotEnoughControllersUnityEvent;

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    // Called by your UI menu
    public void OpenJoinMenu()
    {
        Debug.Log("Join menu opened");
        playerInputManager.EnableJoining();
    }

    // Called when menu closes / game starts
    public void CloseJoinMenu()
    {
        Debug.Log("Join menu closed");
        playerInputManager.DisableJoining();
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        PositionPlayerTransforms(playerInput);
    }

    private void PositionPlayerTransforms(PlayerInput playerInput)
    {
        Transform playerRoot = playerInput.transform.root;

        playerRoot.tag = "Player";
        playerRoot.name = $"Player {playerInput.playerIndex}";
        playerRoot.position = spawnPoints[playerInput.playerIndex].position;

        Debug.Log($"Player {playerInput.playerIndex} joined");
    }
}
