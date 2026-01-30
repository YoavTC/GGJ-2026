using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject joinPrompt;
    [SerializeField] private GameObject playerInfo;
    [SerializeField] private MaskSelectionUI maskSelectionUI;

    public PlayerInput PlayerInput { get; private set; }
    public PlayerLobbyData LobbyData { get; private set; }

    public bool IsOccupied => PlayerInput != null;

    public void AssignPlayer(PlayerInput playerInput)
    {
        PlayerInput = playerInput;
        LobbyData = playerInput.GetComponent<PlayerLobbyData>();

        joinPrompt.SetActive(false);
        playerInfo.SetActive(true);

        maskSelectionUI.Initialize(LobbyData, playerInput);
    }

    public void ClearSlot()
    {
        PlayerInput = null;
        LobbyData = null;

        joinPrompt.SetActive(true);
        playerInfo.SetActive(false);
    }
}
