using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerJoinScript : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
   
    [SerializeField] public List<Transform> spawnPoints = new List<Transform>(); // List of spawn points>
    [SerializeField] private GameObject playerPrefabB;

    // private void OnEnable() => playerInputManager.onPlayerJoined += PositionPlayerTransforms;
    // private void OnDisable() => playerInputManager.onPlayerJoined -= PositionPlayerTransforms;

    public UnityEvent NotEnoughControllersUnityEvent;

    private void Awake()
    {
        AddPlayers();
    }

    public void AddPlayers()
    {
        Debug.Log("Started AddPlayers()");

        var gamepads = Gamepad.all.ToList();

        if (gamepads.Count == 0)
        {
            Debug.Log("Not enough gamepads!");
            NotEnoughControllersUnityEvent?.Invoke();
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = 1f;

        int playerCount = Mathf.Min(
            gamepads.Count + 1,      // allow keyboard as extra
            spawnPoints.Count        // don't exceed spawn points
        );

        for (int i = 0; i < playerCount; i++)
        {
            InputDevice device =
                i < gamepads.Count ? gamepads[i] : Keyboard.current;

            Debug.Log($"Adding player {i} with {device.displayName}");

            var playerInput = playerInputManager.JoinPlayer(
                i,
                -1,
                null,
                device
            );

            PositionPlayerTransforms(playerInput);
        }
    }


    private void PositionPlayerTransforms(PlayerInput playerInput)
    {
        Transform playerParent = playerInput.transform.root;
        Debug.Log($"Positioning player transform {playerParent.gameObject.name}");
        playerParent.gameObject.tag = "Player";
        playerParent.position = spawnPoints[playerInput.playerIndex].position;

        playerParent.gameObject.name = $"player {playerInput.playerIndex}";

       
    }
}