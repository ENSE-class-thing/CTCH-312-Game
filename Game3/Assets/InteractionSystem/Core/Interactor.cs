using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float _castDistance = 6f;
    private Camera playerCamera;

    private IInteractable _currentHeldItem = null;

    private void OnEnable()
    {
        KeyInteraction.OnItemPickedUp += HandleItemPickup;
        PaperInteraction.OnItemPickedUp += HandleItemPickup;
    }

    private void OnDisable()
    {
        KeyInteraction.OnItemPickedUp -= HandleItemPickup;
        PaperInteraction.OnItemPickedUp -= HandleItemPickup;
    }

    private void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No main camera found! Make sure your player camera is tagged 'MainCamera'.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DoInteractionTest(out IInteractable interactable))
            {
                if (interactable.CanInteract())
                {
                    interactable.Interact(this);
                }
            }
        }
    }

    private void HandleItemPickup(IInteractable pickedUpItem)
    {
        if (_currentHeldItem != null && _currentHeldItem != pickedUpItem)
        {
            if (_currentHeldItem is KeyInteraction keyItem)
                keyItem.Drop();
            else if (_currentHeldItem is PaperInteraction paperItem)
                paperItem.Drop();
        }

        _currentHeldItem = pickedUpItem;
    }

    private bool DoInteractionTest(out IInteractable interactable)
    {
        interactable = null;
        if (playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        //Ignores the player (maake the player haave the laayer "Player")
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _castDistance, layerMask))
        {
                        // Check the hit collider first
            interactable = hitInfo.collider.GetComponent<IInteractable>();

            // If not found, search parent objects
            if (interactable == null)
            {
                interactable = hitInfo.collider.GetComponentInParent<IInteractable>();
            }
            return interactable != null;
        }

        return false;
    }

    // =========================
    // GIZMOS FOR DEBUGGING
    // =========================
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.green;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // Draw the ray
        Gizmos.DrawRay(rayOrigin, rayDirection * _castDistance);

        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, _castDistance, layerMask))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
        }
}