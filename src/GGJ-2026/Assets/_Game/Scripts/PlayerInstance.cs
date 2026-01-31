using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstance : MonoBehaviour
{
    [SerializeField] private SelectorUI _selectorUIPrefab;
    public SelectorUI SelectorUI => _selectorUIPrefab;

    [SerializeField] PlayerInput _playerInput;

    [SerializeField] private PlayerController _player;

    private void Awake()
    {
        // Change input to handle the mask selection UI
        //_playerInput.defaultActionMap = ;
        _playerInput.SwitchCurrentActionMap("UI");
        _player.gameObject.SetActive(false); // hide until game scene
    }

    public void OnGameSceneLoad()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        // Wait one frame to ensure the scene is loaded
        yield return null;

        Transform spawnPositionsParent = GameObject.FindGameObjectWithTag("PlayerSpawnPositions").transform;
        Vector3 spawnPosition = spawnPositionsParent.GetChild(_playerInput.playerIndex).position;

        _player.transform.position = spawnPosition;
        _player.gameObject.SetActive(true);
        _playerInput.SwitchCurrentActionMap("Player");
    }
}
