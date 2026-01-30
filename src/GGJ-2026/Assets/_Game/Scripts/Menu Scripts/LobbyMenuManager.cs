using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyMenuManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private Transform[] playerSlots; // size 4
    [SerializeField] private GameObject playerLobbyPrefab;

    private const int MAX_PLAYERS = 4;

    private void OnEnable()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.EnableJoining();
    }

    private void OnDisable()
    {
        inputManager.onPlayerJoined -= OnPlayerJoined;
        inputManager.DisableJoining();
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (PlayerInput.all.Count > MAX_PLAYERS)
        {
            Destroy(playerInput.gameObject);
            return;
        }

        int index = playerInput.playerIndex;

        playerInput.transform.SetParent(playerSlots[index], false);

        var lobbyData = playerInput.gameObject.AddComponent<PlayerLobbyData>();
        lobbyData.playerIndex = index;

        Debug.Log($"Player {index + 1} joined lobby");
    }

    public bool AllPlayersReady()
    {
        foreach (var pi in PlayerInput.all)
        {
            var data = pi.GetComponent<PlayerLobbyData>();
            if (data == null || !data.IsReady)
                return false;
        }
        return true;
    }
}
