using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class FinalDoorInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 _targetRotation = new Vector3(0, -90f, 0f);
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private LeverPuzzleManager puzzleManager;
    private float timer = 0f;
    private bool _isOpen = false;

    private bool hasOpened = false;

    [SerializeField] private AudioClip stoneScraping;
    [SerializeField] private KeyInteraction key;

    private NavMeshObstacle obstacle;
    private Transform player;

    public bool isOpen => _isOpen;

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Target");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;

        // Automatic door closing if player is in the defined boundary
        if (_isOpen && player != null)
        {
            Vector3 p = player.position;

            if (p.z <= -44.5f && p.z >= -134.5f && p.x >= -20f && p.x <= 14.5f)
            {
                CloseDoor();
            }
        }
    }

    public bool CanInteract()
    {
        if (_isOpen || hasOpened)
        {
            UIManager.Instance?.ShowMessage("It won't budge");
            return false;
        }

        if (timer <= 0)
        {
            // Check if correct key is being held
            if (key != null && !key.IsPickedUp)
            {
                UIManager.Instance?.ShowMessage("I need a key");
                return false;
            }

            // Check if puzzleManager assigned is returning TRUE on IsPuzzleSolved
            if (puzzleManager != null && !puzzleManager.IsPuzzleSolved())
            {
                UIManager.Instance?.ShowMessage("Something is still keeping it shut...");
                return false;
            }
            else
            {
                SoundEffectsManager.instance?.PlayOneSecondSoundEffectsClip(stoneScraping, transform, 1f);
                hasOpened = true;
                return true;
            }
        }

        return false;
    }

    public bool Interact(Interactor interactor)
    {
        timer = _rotationSpeed;

        if (_isOpen)
        {
            CloseDoor();
        }
        else
        {
            // Disable obstacle immediately so AI can plan path
            if (obstacle != null)
                obstacle.enabled = false;

            transform.DORotate(_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
            _isOpen = true;
        }

        return true;
    }

    private void CloseDoor()
    {
        transform.DORotate(-_targetRotation, 0.5f, RotateMode.WorldAxisAdd)
            .OnComplete(() =>
            {
                if (obstacle != null)
                    obstacle.enabled = true; // block path again when closed
            });

        _isOpen = false;
    }
}