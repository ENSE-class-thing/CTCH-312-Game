using UnityEngine;
using System; // <-- REQUIRED

public class AltarInteraction : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    [SerializeField] private KeyInteraction requiredKey;

    [SerializeField] private string wrongStateMessage = "Looks like some kind of altar \n";

    public static event Action<AltarInteraction> OnAltarActivated;

    private bool _isActivated = false;

    public bool CanInteract()
    {
        if (_isActivated)
        {
            UIManager.Instance?.ShowMessage("It has already been used.");
            return false;
        }

        if (requiredKey == null)
            return false;

        if (!requiredKey.IsPickedUp)
        {
            UIManager.Instance?.ShowMessage(wrongStateMessage);
            return false;
        }

        return true;
    }

    public bool Interact(Interactor interactor)
    {
        if (!CanInteract())
            return false;

        ActivateAltar();
        return true;
    }

    private void ActivateAltar()
    {
        _isActivated = true;

        requiredKey.Drop();

        requiredKey.transform.SetParent(transform);
        requiredKey.transform.localPosition = new Vector3(0f, 4f, 0f);
        requiredKey.transform.localRotation = Quaternion.identity;

        Rigidbody rb = requiredKey.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var col in requiredKey.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        requiredKey.enabled = false;

        UIManager.Instance?.ShowMessage("THE ALTAR ACCEPTS YOUR OFFERING");

        OnAltarActivated?.Invoke(this);

        Debug.Log("Altar activated with correct item!");
    }
}