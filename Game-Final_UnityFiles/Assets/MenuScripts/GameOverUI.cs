using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject instructionsPanel; // Optional instructions panel

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Ending Texts")]
    [SerializeField] private TextMeshProUGUI playerKilledText;
    [SerializeField] private TextMeshProUGUI badEndingText;
    [SerializeField] private TextMeshProUGUI goodEndingText;

    private void Awake()
    {
        //Make cursor visible and usable again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Hook up button actions
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(MainMenu);
    }

    private void Start()
    {
        ShowEndingText();
    }

    private void ShowEndingText()
    {
        // First, hide all texts
        if (playerKilledText != null) playerKilledText.gameObject.SetActive(false);
        if (badEndingText != null) badEndingText.gameObject.SetActive(false);
        if (goodEndingText != null) goodEndingText.gameObject.SetActive(false);

        // Get the ending type from SceneLoader
        EndingType ending = SceneLoader.Instance.CurrentEnding;

        switch (ending)
        {
            case EndingType.PlayerKilled:
                if (playerKilledText != null) playerKilledText.gameObject.SetActive(true);
                break;

            case EndingType.BadEnding:
                if (badEndingText != null) badEndingText.gameObject.SetActive(true);
                break;

            case EndingType.GoodEnding:
                if (goodEndingText != null) goodEndingText.gameObject.SetActive(true);
                break;
        }
    }

    // Called by Play Again button
    private void PlayGame()
    {
        SceneLoader.Instance.RestartGame();
    }

    // Called by Main Menu button
    private void MainMenu()
    {
        SceneLoader.Instance.LoadMainMenu();
    }
}