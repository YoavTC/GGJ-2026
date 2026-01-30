using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MaskSelectionUI : MonoBehaviour
{
    [SerializeField] private MaskScriptableObjext[] availableMasks;
    [SerializeField] private Image[] selectedMaskIcons;

    private PlayerLobbyData lobbyData;
    private int currentIndex;

    public void Initialize(PlayerLobbyData data, PlayerInput input)
    {
        lobbyData = data;

        // Hook input actions
        input.actions["Navigate"].performed += OnNavigate;
        input.actions["Submit"].performed += OnSelect;
        input.actions["Cancel"].performed += OnDeselect;
    }

    private void OnDestroy()
    {
        // IMPORTANT: unhook when slot is destroyed
        if (lobbyData == null) return;
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        float dir = ctx.ReadValue<Vector2>().x;
        currentIndex = (currentIndex + (dir > 0 ? 1 : -1) + availableMasks.Length)
                       % availableMasks.Length;
    }

    private void OnSelect(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (lobbyData.AddMask(availableMasks[currentIndex]))
            UpdateSelectedUI();
    }

    private void OnDeselect(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || lobbyData.selectedMasks.Count == 0) return;

        lobbyData.RemoveMask(lobbyData.selectedMasks[^1]);
        UpdateSelectedUI();
    }

    private void UpdateSelectedUI()
    {
        for (int i = 0; i < selectedMaskIcons.Length; i++)
        {
            if (i < lobbyData.selectedMasks.Count)
            {
                selectedMaskIcons[i].sprite =
                    lobbyData.selectedMasks[i].MaskPrefab.GetComponent<SpriteRenderer>().sprite;
                selectedMaskIcons[i].enabled = true;
            }
            else
            {
                selectedMaskIcons[i].enabled = false;
            }
        }
    }
}
