using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    // Scene names (match EXACTLY in Build Settings)
    public const string MAIN_MENU = "MainMenu";
    public const string GAME = "MainGame";
    public const string GAME_OVER = "GameOver";
    public EndingType CurrentEnding;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(GAME);
    }

    public void LoadGameOver(EndingType ending)
    {
        CurrentEnding = ending;
        SceneManager.LoadScene(GAME_OVER);
    }
    public void RestartGame()
    {
        LoadGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}