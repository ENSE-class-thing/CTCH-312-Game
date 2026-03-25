public interface IHoldable
{
    void PickUp();
    void Drop();
    bool IsPickedUp { get; }
}