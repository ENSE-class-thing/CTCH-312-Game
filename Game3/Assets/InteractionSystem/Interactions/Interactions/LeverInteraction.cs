using UnityEngine;
using DG.Tweening;
using System;

public class LeverInteraction : MonoBehaviour, IInteractable
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 _targetRotation = new Vector3(0, 0, 60f); // rotation when pulled down
    [SerializeField] private float _rotationSpeed = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip stoneScraping;
    [SerializeField] private bool invertRotation = false;

    private float timer = 0f;
    private bool _isDown = false;

    // Puzzle integration
    private bool _correctState;
    private int _leverIndex;

    public bool IsDown => _isDown;
    public int LeverIndex => _leverIndex;
    public bool IsCorrect => _isDown == _correctState;

    public event Action<LeverInteraction> OnLeverPulled;

    // Store initial rotation
    private Quaternion _startRotation;

    private void Awake()
    {
        _startRotation = transform.localRotation;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;
    }

    // Called by PuzzleManager to initialize lever
    public void Initialize(bool correctState, int index, bool randomizeStart = true)
    {
        _correctState = correctState;
        _leverIndex = index;


        Debug.Log($"Lever {_leverIndex + 1} correct state: {(_correctState ? "DOWN" : "UP")}");
    }

    public bool CanInteract()
    {
        if (timer <= 0)
        {
            SoundEffectsManager.instance?.PlayThreeSecondSoundEffectsClip(stoneScraping, transform, 1f);
            return true;
        }
        return false;
    }

    public bool Interact(Interactor interactor)
{

    timer = _rotationSpeed;

    Vector3 rotationDelta = _isDown 
    ? (invertRotation ? _targetRotation : -_targetRotation) 
    : (invertRotation ? -_targetRotation : _targetRotation);
    
    // Rotate relative to current local rotation around local axes
    transform.DOLocalRotate(transform.localEulerAngles + rotationDelta, _rotationSpeed, RotateMode.FastBeyond360)
        .OnComplete(() =>
        {
            Debug.Log($"Lever {_leverIndex + 1} is {(_isDown ? "UP" : "DOWN")}");
        });

    _isDown = !_isDown;
    OnLeverPulled?.Invoke(this);

    return true;
}
}