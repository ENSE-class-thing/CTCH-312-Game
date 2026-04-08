using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class DrawerInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private float _slideDistance = -0.2f;
    [SerializeField] private float _slideSpeed = 3f;
    private float timer = 0f;
    private bool _isOpen = false;

    private Vector3 _closedPosition;
    private Vector3 _openPosition;

    [SerializeField] private AudioClip drawer;

    private NavMeshObstacle obstacle;

    private void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        _closedPosition = transform.localPosition;
        _openPosition = _closedPosition + new Vector3(_slideDistance, 0, 0);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;
    }

    public bool CanInteract()
    {
        return timer <= 0;
    }

    public bool Interact(Interactor interactor)
    {
        timer = _slideSpeed;
        if (_isOpen)
        {
            // Slide back to closed position
            transform.DOLocalMove(_closedPosition, _slideSpeed)
                .OnComplete(() =>
                {
                    if (obstacle != null)
                        obstacle.enabled = true; // block path again when closed
                });
        }
        else
        {
            // Disable obstacle immediately so AI can pass through
            if (obstacle != null)
                obstacle.enabled = false;

            // Slide to open position
            transform.DOLocalMove(_openPosition, _slideSpeed);
        }

        _isOpen = !_isOpen;

        if(drawer != null){
        SoundEffectsManager.instance?.PlaySoundEffectsClip(drawer, transform, 3f);
        }

        return true;
    }
}