using UnityEngine;
using TMPro;
using System.Collections;

public class LeverClue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeverPuzzleManager puzzleManager; 
    [SerializeField] private TextMeshProUGUI clueText;

    [Header("Which levers to display (indices start at 0)")]
    [SerializeField] private int[] leverIndices;

    private void Start()
    {
        if (puzzleManager != null)
        {
            // Subscribe to puzzle-generated event
            puzzleManager.OnPuzzleGenerated += UpdateClue;
        }
    else
    {
        Debug.Log("puzzleManager is not assigned in LeverClue");
    }

    if (clueText == null)
    {
        Debug.Log("clueText is not assigned in LeverClue");
    }
    }

    public void UpdateClue()
    {
        if (puzzleManager == null || puzzleManager.Solution == null || clueText == null)
            return;

        string numbersLine = "";
        string arrowsLine = "";

        foreach (int i in leverIndices)
        {
            Debug.LogError("Lever " + i + " clue added");
            if (i < 0 || i >= puzzleManager.Solution.Length)
                continue;

            // Add number (1-based)
            numbersLine += (i + 1).ToString() + "\t"; // tab between numbers

            // Add arrow
            bool isDown = puzzleManager.Solution[i]; // true = down
            string arrow = isDown ? "↓" : "↑";
            arrowsLine += arrow + "\t"; // tab between arrows
        }

        // Combine into two-line string
        clueText.text = numbersLine.TrimEnd() + "\n" + arrowsLine.TrimEnd();
        Debug.LogError("Text added to paper");
    }
}