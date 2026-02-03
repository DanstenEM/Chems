using Assets.Scripts.Interactions.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerInteraction : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private ActionToType[] interactActions;

    private readonly HashSet<IInteractable> interactables = new();
    private IInteractable currentInteractable;

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

    void Update()
    {
        RefreshCurrentInteractable();
    }

    void OnTriggerEnter(Collider other)
    {
        var trigger = other.GetComponentInParent<IInteractable>();
        if (trigger == null)
        {
            return;
        }

        interactables.Add(trigger);
        RefreshCurrentInteractable();
    }

    void OnTriggerExit(Collider other)
    {
        var trigger = other.GetComponentInParent<IInteractable>();
        if (trigger == null)
        {
            return;
        }

        interactables.Remove(trigger);
        RefreshCurrentInteractable();
    }

    private void RefreshCurrentInteractable()
    {
        IInteractable closest = null;
        float closestDistance = float.PositiveInfinity;

        if (interactables.Count > 0)
        {
            var currentPosition = transform.position;
            List<IInteractable> toRemove = null;
            foreach (var interactable in interactables)
            {
                if (interactable == null)
                {
                    toRemove ??= new List<IInteractable>();
                    toRemove.Add(interactable);
                    continue;
                }

                var component = interactable as Component;
                if (component == null)
                {
                    continue;
                }

                var distance = (component.transform.position - currentPosition).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }

            if (toRemove != null)
            {
                foreach (var entry in toRemove)
                {
                    interactables.Remove(entry);
                }
            }
        }

        if (closest == currentInteractable)
        {
            return;
        }

        SetCurrentInteractable(closest);
    }

    private void SetCurrentInteractable(IInteractable next)
    {
        if (currentInteractable != null)
        {
            var previousInput = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
            previousInput.Action.action.performed -= OnInteract;
            previousInput.Action.action.Disable();
            currentInteractable.Deactive();
        }

        currentInteractable = next;

        if (currentInteractable == null)
        {
            return;
        }

        var input = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
        input.Action.action.performed += OnInteract;
        input.Action.action.Enable();

        if (input.Action.action.bindings.Count > 0)
        {
            var path = input.Action.action.bindings[0];
            currentInteractable.Active(path);
        }
    }
}

[Serializable]
public struct ActionToType
{
    public KeyActiveType KeyActiveType;
    public  InputActionProperty Action;
}
