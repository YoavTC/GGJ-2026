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

        Gamepad[] gamepads = Gamepad.all.ToArray();
        InputDevice[] inputDevices = new InputDevice[2];

        Debug.Log($"Found {gamepads.Length} gamepads");

        if (gamepads.Length == 0)
        {
            Debug.Log("Not enough gamepads!");
            NotEnoughControllersUnityEvent?.Invoke();
            Time.timeScale = 0f;
            return;
        }

        Debug.Log("Enough gamepads!");

        Time.timeScale = 1f;
        inputDevices[0] = gamepads[0];


        // Set second input as either Gamepad or Keyboard
        if (gamepads.Length == 2)
        {
            Debug.Log("Setting 2nd controller to gamepad");
            inputDevices[1] = gamepads[1];
        }
        else
        {
            Debug.Log("Setting 2nd controller to keyboard");
            inputDevices[1] = Keyboard.current;
        }

        // Join the players with the correct input devices
        for (int i = 0; i < inputDevices.Length; i++)
        {
            Debug.Log($"Adding player {i}");
            var playerInput = playerInputManager.JoinPlayer(i, -1, null, inputDevices[i]);
            playerInputManager.playerPrefab = playerPrefabB;

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