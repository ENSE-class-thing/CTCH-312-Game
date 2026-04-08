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

        string arrowsLine = "";

        foreach (int i in leverIndices)
        {
            if (i < 0 || i >= puzzleManager.Solution.Length)
                continue;

            bool isDown = puzzleManager.Solution[i];
            string arrow = isDown ? "↓" : "↑";
            arrowsLine += arrow + "\t";
        }

        arrowsLine = arrowsLine.TrimEnd();

        // Split existing text into lines
        string[] lines = clueText.text.Split('\n');

        // Ensure at least 3 lines exist
        if (lines.Length < 3)
        {
            System.Array.Resize(ref lines, 3);
        }

        // Replace ONLY the 3rd line (index 2)
        lines[2] = arrowsLine;

        // Rebuild the text
        clueText.text = string.Join("\n", lines);

    }
}