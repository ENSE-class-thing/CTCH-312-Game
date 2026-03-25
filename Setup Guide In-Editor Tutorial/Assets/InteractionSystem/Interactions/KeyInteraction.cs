using UnityEngine;
using DG.Tweening;

public class KeyInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private float _followSpeed = 50f; // Speed for following the player
    [SerializeField] private Vector3 _handOffset = new Vector3(0f, -0.5f, 2f); // Offset in front of camera
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0f, 0f, 0f); // Optional rotation offset for natural look
    private Vector3 _originalPosition; // Original position
    private Quaternion _originalRotation; // Original rotation
    private bool _isPickedUp = false;
    public bool IsPickedUp => _isPickedUp;
    private Transform _playerCamera;

    [HideInInspector] public bool HasKey = false;

    private Rigidbody _rigidbody;

    private void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            // Add a Rigidbody if it doesn't exist
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.isKinematic = true; // Start kinematic because key is initially static
        }

        Drop();
    }

    public bool CanInteract()
    {
        return true;
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

    private void Update()
{
    if (_isPickedUp && _playerCamera != null)
    {
        // Follow camera position + offset
        Vector3 targetPosition = _playerCamera.position 
                               + _playerCamera.forward * _handOffset.z
                               + _playerCamera.up * _handOffset.y
                               + _playerCamera.right * _handOffset.x;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, _followSpeed * Time.deltaTime);

        // Rotate with camera
        transform.rotation = _playerCamera.rotation * Quaternion.Euler(_rotationOffset);

        // Drop with Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    // Check if key has fallen out of bounds
    if (transform.position.y < -50f)
    {
        ResetToOriginalPosition();
    }
}

    private void PickUp()
    {
        _isPickedUp = true;
        HasKey = true;

        // Find the main camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            _playerCamera = cam.transform;
        }
        else
        {
            Debug.LogWarning("No camera with tag 'MainCamera' found!");
        }

        // Disable collider while held
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Disable gravity and make kinematic while held
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        Debug.Log("Key picked up!");
    }

    private void Drop()
    {
        _isPickedUp = false;
        HasKey = false;
        _playerCamera = null;

        // Enable physics so key falls naturally
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        // Re-enable collider
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Debug.Log("Key dropped!");
    }

    // Optional helper: teleport key back to original position if needed
public void ResetToOriginalPosition()
{
    _rigidbody.isKinematic = true;
    _rigidbody.useGravity = false;
    
    // Reset velocity safely using linearVelocity
    #if UNITY_2023_1_OR_NEWER
        _rigidbody.linearVelocity = Vector3.zero;
    #else
        _rigidbody.velocity = Vector3.zero;
    #endif

    transform.position = _originalPosition;
    transform.rotation = _originalRotation;

    _isPickedUp = false;
    HasKey = false;
    _playerCamera = null;
}
}