using UnityEngine;
using System.Linq;
using System;
using System.Collections;

public class LeverPuzzleManager : MonoBehaviour
{
    [SerializeField] private LeverInteraction[] levers;

    private bool[] solution;
    //Need to make a public solution aarray so that the clues can sync up with the randomly generated
    //puzzle solutions
    public bool[] Solution => solution;
    public event Action OnPuzzleGenerated;
    private void Start()
    {
        GenerateSolution();
        AssignSolutionToLevers();

        foreach (var lever in levers)
        {
            lever.OnLeverPulled += HandleLeverPulled;
        }

        StartCoroutine(DelayedEventFiring());
        OnPuzzleGenerated?.Invoke();
    }

    private IEnumerator DelayedEventFiring()
    {
        // Wait for 1 second before firing the event
        yield return new WaitForSeconds(1f);

        // Fire the event after the delay
        OnPuzzleGenerated?.Invoke();
        Debug.Log("OnPuzzleGenerated event fired");
    }


    private void GenerateSolution()
    {
        solution = new bool[levers.Length];

        for (int i = 0; i < solution.Length; i++)
        {
            solution[i] = UnityEngine.Random.value > 0.5f;
        }

        // Prevent all same (optional but recommended)
        if (solution.All(x => x) || solution.All(x => !x))
        {
            GenerateSolution();
        }
    }

    private void AssignSolutionToLevers()
    {
        for (int i = 0; i < levers.Length; i++)
        {
            levers[i].Initialize(solution[i], i, true);
        }
    }

    public bool GetSolutionForLever(int index)
    {
        return solution[index];
    }

    public bool IsPuzzleSolved()
    {
        foreach (var lever in levers)
        {
            if (!lever.IsCorrect)
                return false;
        }

        Debug.Log("PUZZLE SOLVED!");
        return true;
    }

    private void HandleLeverPulled(LeverInteraction lever)
    {
        Debug.Log($"Lever {lever.LeverIndex + 1} pulled. Checking puzzle...");

        if (IsPuzzleSolved())
        {
            OnPuzzleSolved();
        }
    }

    private void OnPuzzleSolved()
    {
        Debug.Log("PUZZLE SOLVED!");

        // Example:
        // door.Open();
        // play sound
        // trigger animation
    }
}