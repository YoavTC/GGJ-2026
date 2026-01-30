using System.Collections.Generic;
using UnityEngine;

public class PlayerLobbyData : MonoBehaviour
{
    public int playerIndex;
    public List<MaskScriptableObjext> selectedMasks = new List<MaskScriptableObjext>(3);

    public bool IsReady => selectedMasks.Count == 3;

    public bool AddMask(MaskScriptableObjext mask)
    {
        if (selectedMasks.Count >= 3 || selectedMasks.Contains(mask))
            return false;

        selectedMasks.Add(mask);
        return true;
    }

    public void RemoveMask(MaskScriptableObjext mask)
    {
        selectedMasks.Remove(mask);
    }
}
