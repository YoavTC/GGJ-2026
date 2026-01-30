using UnityEngine;
using UnityEngine.InputSystem;

public class MaskSelectionController : MonoBehaviour
{
    [SerializeField] private MaskScriptableObjext[] availableMasks; // size 5

    private PlayerLobbyData lobbyData;
    private int currentIndex = 0;

    private void Awake()
    {
        lobbyData = GetComponent<PlayerLobbyData>();
    }

    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        float dir = ctx.ReadValue<Vector2>().x;
        currentIndex = (currentIndex + (dir > 0 ? 1 : -1) + availableMasks.Length)
                       % availableMasks.Length;
    }

    public void OnSelect(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        var mask = availableMasks[currentIndex];
        lobbyData.AddMask(mask);
    }

    public void OnDeselect(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || lobbyData.selectedMasks.Count == 0) return;

        lobbyData.RemoveMask(
            lobbyData.selectedMasks[^1]
        );
    }
}
