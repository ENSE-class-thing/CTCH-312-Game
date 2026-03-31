using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 _targetRotation = new Vector3(0, -90f, 0f);
    [SerializeField] private float _rotationSpeed = 3f;
    [SerializeField] private LeverPuzzleManager puzzleManager;
    private float timer = 0f;
    private bool _isOpen = false;

    [SerializeField] private AudioClip stoneScraping;
    [SerializeField] private KeyInteraction key;

    private NavMeshObstacle obstacle;

    public bool isOpen => _isOpen;

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;
    }

    public bool CanInteract()
    {
        if(_isOpen){
            UIManager.Instance?.ShowMessage("It won't budge");
            return false;
        }
        if (timer <= 0)
        {
            //Check if correct key is being held
            if (key != null && !key.IsPickedUp)
            {
                UIManager.Instance?.ShowMessage("I need a key");
                return false;
            }

            //Check if puzzleManager assigned is retunrning TRUE on ispuzzlesolved
            if (puzzleManager != null && !puzzleManager.IsPuzzleSolved())
            {
                UIManager.Instance?.ShowMessage("Something is still keeping it shut...");
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
        timer = _rotationSpeed;

        if (_isOpen)
        {
            transform.DORotate(-_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd)
                .OnComplete(() =>
                {
                    obstacle.enabled = true; // block path again when closed
                });
        }
        else
        {
            // Disable obstacle immediately so AI can plan path
            if (obstacle != null)
                obstacle.enabled = false;

            transform.DORotate(_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
        }

        _isOpen = !_isOpen;

        return true;
    }
}