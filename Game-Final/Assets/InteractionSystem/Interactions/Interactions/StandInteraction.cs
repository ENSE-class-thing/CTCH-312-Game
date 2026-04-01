using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class StandInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 _targetTransform = new Vector3(0, -10f, 0f);
    private Vector3 _enable = new Vector3(0f, 0f, 0f);
    [SerializeField] private float _transformSpeed = 7f;
    [SerializeField] private LeverPuzzleManager puzzleManager;
    private float timer = 0f;
    private bool _isOpen = false;
    [SerializeField] private int correctBookIndex;

    [SerializeField] private AudioClip stoneScraping;
    [SerializeField] private BookInteraction book;
    [SerializeField] private int activated;

    public bool isOpen => _isOpen;

    private NavMeshObstacle obstacle;

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnEnable()
    {
        if (puzzleManager != null)
            puzzleManager.OnPuzzleGenerated += AssignBook;
    }

    private void OnDisable()
    {
        if (puzzleManager != null)
            puzzleManager.OnPuzzleGenerated -= AssignBook;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;
        if((activated == 0 && puzzleManager.IsPuzzleSolved()) || activated == 1)
        {
            transform.DOLocalMove(_enable, _transformSpeed);
            activated = 2;
        }

    }

    public bool CanInteract()
    {
        if (timer <= 0)
        {
            //Check if correct key is being held
            if (book != null && !book.IsPickedUp)
            {
                UIManager.Instance?.ShowMessage("I need the right book");
                return false;
            }

            else
            {
                SoundEffectsManager.instance?.PlayThreeSecondSoundEffectsClip(stoneScraping, transform, 1f);
                return true;
            }
        }
        return false;
    }

public bool Interact(Interactor interactor)
{
    timer = _transformSpeed;

    // Check if the correct book is assigned and is being held
    if (book != null && book.IsPickedUp)
    {
        // Stop the book from being held
        book.Drop();
        Rigidbody rb = book.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;   // stops all physics simulation
            rb.useGravity = false;   // disable gravity
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = book.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Parent it to the stand
        book.transform.SetParent(transform);

        // Set local position & rotation so it sits nicely on the stand
        Vector3 temp = new Vector3(0.9f, 6.2f, 0); // adjust Y as needed
        book.transform.localPosition = temp;
        book.transform.localRotation = Quaternion.Euler(0f, 180f, -45f);

        // Disable book colliders so it can't be interacted with anymore


        // Disable the script to prevent picking it up again
        book.enabled = false;

        // Disable physics completely


        Debug.Log($"{book.name} placed on {name}!");
    }

    // Disable obstacle immediately so AI can plan path
    if (obstacle != null)
        obstacle.enabled = false;

    transform.DOLocalMove(_targetTransform, _transformSpeed);

    _isOpen = true;

    return true;
}

    private void AssignBook()
    {
        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager not assigned!");
            return;
        }

        var correctBooks = puzzleManager.CorrectBooks;

        if (correctBooks == null || correctBooks.Length <= correctBookIndex)
        {
            Debug.LogError($"Invalid book index {correctBookIndex} on {name}");
            return;
        }

        book = correctBooks[correctBookIndex];

        Debug.Log($"{name} assigned book: {book.name}");
    }
}