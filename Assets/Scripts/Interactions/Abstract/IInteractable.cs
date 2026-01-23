using Assets.Scripts.Interactions.Abstract;
using UnityEngine.InputSystem;

public interface IInteractable
{
    public abstract KeyActiveType keyType { get; set; }
    public void Interact();
    public void Active(InputBinding input);
    public void Deactive();
}