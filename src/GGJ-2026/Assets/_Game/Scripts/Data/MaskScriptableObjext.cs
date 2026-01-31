using UnityEngine;

[CreateAssetMenu(fileName = "Masks")]
public class MaskScriptableObjext : ScriptableObject
{
    public MaskType MaskType;
    public Sprite MaskIcon;
    public GameObject MaskPrefab;
    public string MaskName;
    [TextArea] public string Description;
    public float maskCooldown;

}

public enum MaskType
{
    HEAVY,
    DEFLECT,
    DASH,
    ANCHOR,
    KAMIKAZE,
    STUN
}
