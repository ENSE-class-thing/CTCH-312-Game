using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class DoorInteractionHinge : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 _targetRotation = new Vector3(0, -90f, 0f);
    [SerializeField] private float _rotationSpeed = 1f;

    [SerializeField] private bool isInverted = false;
    private float timer = 0f;
    private bool _isOpen = false;

    [SerializeField] private AudioClip stoneScraping;

    private NavMeshObstacle obstacle;
    // 🔥 Track original state for path simulation
    private bool originalObstacleState;

    public NavMeshObstacle Obstacle => obstacle; // read-only property

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle == null)
            obstacle = GetComponentInChildren<NavMeshObstacle>();

        if (obstacle != null)
        {
            obstacle.carving = true; 
            originalObstacleState = obstacle.enabled;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;
    }

    public bool CanInteract()
    {
        if (timer <= 0)
        {
            SoundEffectsManager.instance?.PlayOneSecondSoundEffectsClip(stoneScraping, transform, 1f);
            return true;
        }
        return false;
    }

   public bool Interact(Interactor interactor)
    {
        timer = _rotationSpeed;

    if (_isOpen && isInverted == false)
    {
        transform.DORotate(-_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
    }
    else if (_isOpen && isInverted == true)
    {
        transform.DORotate(_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
    }
    else if(_isOpen == false && isInverted == false)
    {
        transform.DORotate(_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
    }
    else if(_isOpen == false && isInverted == true)
    {
        transform.DORotate(-_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
    }

        _isOpen = !_isOpen;
        return true;
    }

    public bool IsOpen()
    {
        return _isOpen;
    }

    // =========================
    // 🧠 SAFE PATH SIMULATION
    // =========================
    public void ForceOpenForPathing(bool open)
    {
        if (obstacle == null) return;

        if (open)
        {
            // Temporarily disable obstacle
            obstacle.enabled = false;
        }
        else
        {
            // Restore original state (based on real door state)
            obstacle.enabled = !_isOpen;
        }
    }
}