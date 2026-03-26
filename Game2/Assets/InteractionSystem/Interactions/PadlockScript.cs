using UnityEngine;

public class PadlockScript : MonoBehaviour
{
    private DoorInteraction door; // Reference to the parent door
    private Rigidbody _rigidbody;

    private void Start()
    {
        // Get the DoorInteraction component from the parent (the door)
        door = GetComponentInParent<DoorInteraction>();

        // Ensure the Rigidbody is cached
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Check if the door is open and the padlock has not already been dropped
        if (door != null && door.isOpen)
        {
            Drop();
        }
    }

    private void Drop()
    {

        // Detach the padlock from the door (make it a world object)
        transform.SetParent(null);  // Remove padlock from being a child of the door


        // Ensure the Rigidbody is not kinematic, and gravity is applied
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        // Enable the collider if it's disabled
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;





        Debug.Log("Padlock dropped!"); 
    }
}