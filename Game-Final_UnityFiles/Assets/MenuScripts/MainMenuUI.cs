using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject instructionsPanel; // Assign your Instructions Panel here
    [SerializeField] private GameObject backstoryPanel;
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button quitButton;
    private bool waitingForEnter = false;


    private void Awake()
    {
        // Hook up button actions
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (instructionsButton != null)
            instructionsButton.onClick.AddListener(ShowInstructions);

        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(HideInstructions);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Ensure instructions panel is hidden at start
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        if (backstoryPanel != null)
            backstoryPanel.SetActive(false);

    }


    private System.Collections.IEnumerator WaitForBackstoryContinue()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        //Wait until player presses Enter
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        waitingForEnter = false;
        SceneLoader.Instance.LoadGame();
    }


    // Called by Play button
    public void PlayGame()
    {
        if (backstoryPanel != null)
        {
            backstoryPanel.SetActive(true);
            waitingForEnter = true;
            StartCoroutine(WaitForBackstoryContinue());
        }
    }

    // Called by Instructions button
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    // Called by Close button on Instructions panel
    public void HideInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    // Called by Quit button
    public void QuitGame()
    {
        SceneLoader.Instance.QuitGame();
    }
}