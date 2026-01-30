using UnityEngine;

public class MaskButtonUI : MonoBehaviour
{
    public MaskScriptableObjext mask;

    [SerializeField] private UnityEngine.UI.Image icon;
    [SerializeField] private GameObject selectedOverlay;

    public void Setup(MaskScriptableObjext data)
    {
        mask = data;
        Debug.Log(data.MaskPrefab
            .GetComponent<SpriteRenderer>().sprite);
        icon.sprite = data.MaskPrefab
            .GetComponent<SpriteRenderer>().sprite;
    }

    private void Awake()
    {
        icon.sprite = mask.MaskPrefab
           .GetComponent<SpriteRenderer>().sprite;
    }

    public void SetHighlighted(bool value)
    {
        // glow / outline / scale
    }

    public void SetSelected(bool value)
    {
        selectedOverlay.SetActive(value);
    }

    public void SetDisabled(bool value)
    {
        // greyscale, lock icon, etc
    }
}
