using UnityEngine;
using TMPro;

public class BookStandClue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeverPuzzleManager puzzleManager;
    [SerializeField] private TextMeshProUGUI clueText;

    [Header("Which correct book index to display")]
    [SerializeField] private int correctBookIndex; // 0, 1, etc.

    private void Start()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleGenerated += UpdateBookClue;
        }
        else
        {
            Debug.LogError("PuzzleManager not assigned in BookStandClue");
        }

        if (clueText == null)
        {
            Debug.LogError("ClueText not assigned in BookStandClue");
        }
    }

    private void UpdateBookClue()
    {
        if (puzzleManager == null || clueText == null)
            return;

        var correctBooks = puzzleManager.CorrectBooks;

        if (correctBooks == null || correctBooks.Length <= correctBookIndex)
        {
            Debug.LogError($"Invalid correctBookIndex {correctBookIndex} on {name}");
            return;
        }

        var book = correctBooks[correctBookIndex];
        int index = puzzleManager.GetBookIndex(book);

        if (index < 0)
        {
            Debug.LogError("Book not found in allBooks!");
            return;
        }

        // Display (1-based for player friendliness)
        clueText.text = (index + 1).ToString();

        Debug.Log($"{name} displays book index: {index}");
    }

    private void OnDestroy()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleGenerated -= UpdateBookClue;
        }
    }
}