using Assets.Scripts.Interactions.Abstract;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerInteraction : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private ActionToType[] interactActions;

    IInteractable currentInteractable;

    void OnDestroy()
    {
        foreach (var item in interactActions)
        {
            item.Action.action.performed -= OnInteract;
        }
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var trigger = other.GetComponent<IInteractable>();
        if (trigger != null)
        {
            currentInteractable = trigger;

            var input = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
            input.Action.action.performed += OnInteract;
            input.Action.action.Enable();

            var path = input.Action.action.bindings[0];

            currentInteractable.Active(path);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var trigger = other.GetComponent<IInteractable>();
        if (trigger != null && trigger.Equals(currentInteractable))
        {
            var input = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
            input.Action.action.performed -= OnInteract;
            input.Action.action.Disable();
            currentInteractable.Deactive();

            currentInteractable = null;
        }
    }
}

[Serializable]
public struct ActionToType
{
    public KeyActiveType KeyActiveType;
    public  InputActionProperty Action;
}