using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }

    [SerializeField] private string mainMenuScene;
    [SerializeField] private string maskSelectScene;
    [SerializeField] private string gameScene;

    [SerializeField] private GameState defaultGameState;
    private GameState gameState;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        ChangeState(defaultGameState);
    }

    public void ChangeState(GameState newState)
    {
        if (newState == gameState) return;
        gameState = newState;
        switch (gameState)
        {
            case GameState.MAIN_MENU:
                SceneManager.LoadScene(mainMenuScene);
                break;
            case GameState.MASK_SELECT:
                SceneManager.LoadScene(maskSelectScene);
                break;
            case GameState.IN_GAME:
                SceneManager.LoadScene(gameScene);
                PlayerInstance[] players = FindObjectsByType<PlayerInstance>(FindObjectsSortMode.None);
                foreach (PlayerInstance player in players) player.OnGameSceneLoad();
                break;
        }
    }
}
public enum GameState
{
    MAIN_MENU,
    MASK_SELECT,
    IN_GAME
}