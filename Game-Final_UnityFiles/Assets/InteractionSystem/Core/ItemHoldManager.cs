using UnityEngine;

public class ItemHoldManager : MonoBehaviour
{
    public static ItemHoldManager Instance;

    private IHoldable _currentItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //Called when an item is picked up
    public void PickUpItem(IHoldable newItem)
    {
        if (_currentItem != null && _currentItem != newItem)
        {
            _currentItem.Drop(); //Drop currently held item
        }

        _currentItem = newItem;
    }

    //Called when an item is dropped manually
    public void ClearItem(IHoldable item)
    {
        if (_currentItem == item)
        {
            _currentItem = null;
        }
    }
}