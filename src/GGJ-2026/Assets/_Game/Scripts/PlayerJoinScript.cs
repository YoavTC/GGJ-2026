using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinScript : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private List<Transform> spawnPoints;

    private int nextSpawnIndex = 0;

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (nextSpawnIndex >= spawnPoints.Count)
        {
            Debug.LogWarning("No spawn point available for player");
            return;
        }

        Transform spawnPoint = spawnPoints[nextSpawnIndex];

        Transform playerRoot = playerInput.transform;
        playerRoot.position = spawnPoint.position;
        playerRoot.rotation = spawnPoint.rotation;

        playerRoot.tag = "Player";
        playerRoot.name = $"Player {playerInput.playerIndex}";

        Debug.Log($"Player {playerInput.playerIndex} spawned at {spawnPoint.name}");

        nextSpawnIndex++;
    }
}
