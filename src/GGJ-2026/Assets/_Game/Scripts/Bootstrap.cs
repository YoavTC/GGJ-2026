using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string maskSelectScene;
    [SerializeField] private string gameScene;

    private enum GameState
    {
        MAIN_MENU,
        MASK_SELECT,
        IN_GAME
    }

    private GameState gameState;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        SceneManager.LoadScene(mainMenuScene);
        gameState = GameState.MAIN_MENU;
    }

    private void Update()
    {
        if (gameState == GameState.MAIN_MENU)
        {
            if (Input.anyKeyDown)
            {
                SceneManager.LoadScene(maskSelectScene);
                gameState = GameState.MASK_SELECT;
            }
        }
    }
}
