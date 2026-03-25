using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float _castDistance = 5f;
    private Camera playerCamera;

    private void Start()
    {
        // Automatically find the main camera
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

    private bool DoInteractionTest(out IInteractable interactable)
    {
        interactable = null;

        if (playerCamera == null) return false;

        // Ray starts from the camera's position and points forward
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _castDistance))
        {
            interactable = hitInfo.collider.GetComponent<IInteractable>();
            return interactable != null;
        }

        return false;
    }
}