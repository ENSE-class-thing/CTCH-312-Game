using UnityEngine;
using DG.Tweening;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Vector3 _targetRotation = new Vector3(0, -100f, 0f);
    [SerializeField]
    private float _rotationSpeed = 3f;
    private float timer = 0f;
    private bool _isOpen = false;

    [SerializeField] private AudioClip stoneScraping;

    [SerializeField] private KeyInteraction key;

    private void Update(){

        timer -= Time.deltaTime;

        if(timer < 0)
        {
            timer = 0;
        }
    }
    
        public bool CanInteract()
    {

        // Only allow interaction if key is picked up
        if(timer<=0){
            if (key != null && key.IsPickedUp)
            {
                SoundEffectsManager.instance?.PlayThreeSecondSoundEffectsClip(stoneScraping, transform, 1f);
                return true;
            }
            else
            {

               Debug.Log("Door requires key!");
               return false;
            }
        }
        return false;

    }

    public bool Interact(Interactor interactor)
    {
        timer = _rotationSpeed;
        if (_isOpen)
        {
            transform.DORotate(-_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
        }
        else
        {
            transform.DORotate(_targetRotation, _rotationSpeed, RotateMode.WorldAxisAdd);
        }

        _isOpen = !_isOpen;

        return true;
    }

}
