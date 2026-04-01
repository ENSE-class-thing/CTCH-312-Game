using UnityEngine;
using System;

public class PadlockScript : MonoBehaviour
{
    private DoorInteraction door; // Reference to the parent door

    [SerializeField] private AudioClip _dropSound; // Sound to play when it hits the ground
    private DoorInteractionHinge doorHinge;
    private FinalDoorInteraction doorFinal;
    private Rigidbody _rigidbody;

    private bool hasDropped = false;

    public static event Action<Vector3> OnDropSound;

    private void Start()
    {
        // Get the DoorInteraction component from the parent (the door)
        door = GetComponentInParent<DoorInteraction>();
        doorFinal = GetComponentInParent<FinalDoorInteraction>();
        doorHinge = GetComponentInParent<DoorInteractionHinge>();

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

        if (doorHinge != null && doorHinge.isOpen)
        {
            Drop();
        }

        if (doorFinal != null && doorFinal.isOpen)
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





        //Debug.Log("Padlock dropped!"); 
    }

        public void OnCollisionEnter(Collision collision)
    {
        // Check if the key is not picked up, i.e., it's resting on the ground or other objects
        if (!hasDropped && _dropSound != null)
        {
            SoundEffectsManager.instance?.PlaySoundEffectsClip(_dropSound, transform, 0.5f);
            OnDropSound?.Invoke(transform.position);
            UIManager.Instance?.ShowMessage("I think he heard that...");
            hasDropped = true;
        }
    }


}