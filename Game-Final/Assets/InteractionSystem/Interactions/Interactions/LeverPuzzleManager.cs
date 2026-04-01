using UnityEngine;
using System.Linq;
using System;
using System.Collections;

public class LeverPuzzleManager : MonoBehaviour
{
    [SerializeField] private LeverInteraction[] levers;
    [SerializeField] private BookInteraction[] allBooks;
    [SerializeField] private BookInteraction[] correctBooks;

    [SerializeField] private AudioClip stoneScraping;

    public BookInteraction[] CorrectBooks => correctBooks;

    private bool[] solution;

    // Expose solution so clues or other scripts can reference it
    public bool[] Solution => solution;

    public event Action OnPuzzleGenerated;

    private void Start()
    {
        GenerateSolution();
        // Assign only the lever index and subscribe to events, not the correct state
        AssignLevers();

        GenerateCorrectBooks();

        StartCoroutine(DelayedEventFiring());
        OnPuzzleGenerated?.Invoke();
    }

    private IEnumerator DelayedEventFiring()
    {
        yield return new WaitForSeconds(1f);
        OnPuzzleGenerated?.Invoke();
        Debug.Log($"{name} OnPuzzleGenerated event fired");
    }

    private void GenerateSolution()
    {
        solution = new bool[levers.Length];

        for (int i = 0; i < solution.Length; i++)
        {
            solution[i] = UnityEngine.Random.value > 0.5f;
        }

        // Prevent all same (optional)
        if (solution.All(x => x) || solution.All(x => !x))
        {
            GenerateSolution();
        }

        Debug.Log($"{name} generated solution: {string.Join(",", solution.Select(b => b ? "1" : "0"))}");
    }

    private void AssignLevers()
    {
        for (int i = 0; i < levers.Length; i++)
        {
            levers[i].Initialize(false, i); // just give the lever its index, not the solution
            levers[i].OnLeverPulled += HandleLeverPulled;
        }
    }

    // Check solution based on lever states, not their internal correct state
    public bool IsPuzzleSolved()
    {
        for (int i = 0; i < levers.Length; i++)
        {
            if (levers[i].IsDown != solution[i])
                return false;
        }

        Debug.Log($"{name} PUZZLE SOLVED!");
        return true;
    }

    private void HandleLeverPulled(LeverInteraction lever)
    {
        Debug.Log($"Lever {lever.LeverIndex + 1} pulled in {name}. Checking puzzle...");

        if (IsPuzzleSolved())
        {
            OnPuzzleSolved();
        }
    }

    private void OnPuzzleSolved()
    {
        Debug.Log($"{name} PUZZLE SOLVED!");
        if(allBooks != null && allBooks.Length > 0)
        {
            UIManager.Instance?.ShowMessage("I heard something moving in the first room");
            SoundEffectsManager.instance?.PlayThreeSecondSoundEffectsClip(stoneScraping, transform, 1f);
        }

        // Trigger whatever needs to happen (door, sound, etc.)
    }

    // Optionally: get the target solution for a lever
    public bool GetSolutionForLever(int index)
    {
        return solution[index];
    }

    private void GenerateCorrectBooks()
    {
        if (allBooks == null || allBooks.Length < 2)
        {
            Debug.LogError("Not enough books assigned!");
            return;
        }

        // Shuffle and take first 2
        var shuffled = allBooks.OrderBy(x => UnityEngine.Random.value).ToArray();

        correctBooks = new BookInteraction[2];
        correctBooks[0] = shuffled[0];
        correctBooks[1] = shuffled[1];

        Debug.Log($"{name} selected correct books: {correctBooks[0].name}, {correctBooks[1].name}");
    }

    public int GetBookIndex(BookInteraction book)
    {
        for (int i = 0; i < allBooks.Length; i++)
        {
            if (allBooks[i] == book)
                return i;
        }

        return -1;
    }
}