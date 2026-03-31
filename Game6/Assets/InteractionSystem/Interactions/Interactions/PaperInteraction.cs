using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PaperInteraction : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    [SerializeField] private float _followSpeed = 50f; // Speed for following the player
    [SerializeField] private Vector3 _handOffset = new Vector3(0f, -1f, 0.5f); // Offset in front of camera
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero; // Optional rotation offset

    [Header("Reset Settings")]
    [SerializeField] private float _fallThresholdY = -50f;
    public static event Action<IInteractable> OnItemPickedUp;


    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private Rigidbody _rigidbody;

    private bool _isPickedUp = false;
    public bool IsPickedUp => _isPickedUp;
    [HideInInspector] public bool IsHeld => _isPickedUp;
    public static event Action<Vector3> OnDropSound;

    private Transform _playerCamera;

    // Dynamic parent-following variables
    private Transform _currentParent;
    private Vector3 _lastParentPosition;

    [SerializeField] private AudioClip _dropSound; // Sound to play when it hits the ground
    private AudioSource _audioSource;


    private void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = gameObject.AddComponent<Rigidbody>();

        // Start dropped in the world
        _isPickedUp = false;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        _currentParent = null;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;

    }

    private void OnEnable()
    {
        Enemy_AI_Script.OnBadEnding += DropPaper;
    }

    private void OnDisable()
    {
        Enemy_AI_Script.OnBadEnding -= DropPaper;
    }

    private void DropPaper()
    {
        StartCoroutine(DropPaperAfterDelay(1.5f)); // drop after 1 second
    }

    private IEnumerator DropPaperAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Drop();
    }
    private void Update()
    {
        // --- PICKED UP BY PLAYER ---
        if (_isPickedUp && _playerCamera != null)
        {
            Vector3 targetPos = _playerCamera.position
                                + _playerCamera.forward * _handOffset.z
                                + _playerCamera.up * _handOffset.y
                                + _playerCamera.right * _handOffset.x;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, _followSpeed * Time.deltaTime);
            transform.rotation = _playerCamera.rotation * Quaternion.Euler(_rotationOffset);

            // Drop with Q
            if (Input.GetKeyDown(KeyCode.Q))
                Drop();
        }
        // --- DROPPED OR RESTING ON OBJECT ---
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f))
            {
                if (hit.transform != null && hit.transform != _currentParent)
                {
                    _currentParent = hit.transform;
                    _lastParentPosition = _currentParent.position;
                }
            }
            else
            {
                _currentParent = null;
            }

            if (_currentParent != null)
            {
                Vector3 parentDelta = _currentParent.position - _lastParentPosition;
                transform.position += parentDelta;
                _lastParentPosition = _currentParent.position;
            }
        }

        // Reset if out of bounds
        if (transform.position.y < _fallThresholdY)
            ResetToOriginalPosition();
    }

    public bool CanInteract()
    {
        return true;
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Check if the key is not picked up, i.e., it's resting on the ground or other objects
        if (!_isPickedUp && _dropSound != null)
        {
            SoundEffectsManager.instance?.PlaySoundEffectsClip(_dropSound, transform, 0.5f);
            OnDropSound?.Invoke(transform.position);
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (!_isPickedUp)
        {
            PickUp();
            return true;
        }
        return false;
    }

    private void PickUp()
    {
        _isPickedUp = true;
        _currentParent = null;

        Camera cam = Camera.main;
        if (cam != null)
            _playerCamera = cam.transform;
        else
            Debug.LogWarning("No camera with tag 'MainCamera' found!");

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        OnItemPickedUp?.Invoke(this);
        Debug.Log("Paper picked up!");
    }

    public void Drop()
    {
        _isPickedUp = false;
        _playerCamera = null;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log("Paper dropped!");
    }

    public void ResetToOriginalPosition()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

#if UNITY_2023_1_OR_NEWER
        _rigidbody.linearVelocity = Vector3.zero;
#else
        _rigidbody.velocity = Vector3.zero;
#endif

        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        _isPickedUp = false;
        _playerCamera = null;
        _currentParent = null;

        Debug.Log("Paper reset to original position.");
    }
}