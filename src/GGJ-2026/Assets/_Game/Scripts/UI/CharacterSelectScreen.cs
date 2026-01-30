using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectScreen : MonoBehaviour
{
    [SerializeField] private List<GameObject> masks;

    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private Transform _masksGridParentTransform;

    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Color[] _playerColors;
    private int _nextColorIndex = 0;
    public (Transform, Transform, Color, Sprite) GetReferences()
    {
        Color color = _playerColors[_nextColorIndex];
        _nextColorIndex++;
        return (_canvasTransform, _masksGridParentTransform, color, _sprites[_nextColorIndex]);
    }

    public void SelectMask(GameObject mask)
    {

    }
}
