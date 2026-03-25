using UnityEngine;

public class DoorInteractProxy : MonoBehaviour, IInteractable
{
    private DoorInteractionHinge hinge;

    private void Awake()
    {
        hinge = GetComponentInParent<DoorInteractionHinge>();
    }

    public bool CanInteract()
    {
        return hinge != null && hinge.CanInteract();
    }

    public bool Interact(Interactor interactor)
    {
        return hinge != null && hinge.Interact(interactor);
    }
}