using Assets.Scripts.Interactions.Abstract;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerInteraction : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private ActionToType[] interactActions;

    [Header("Targeting")]
    [SerializeField] private float lookRayDistance = 6f;
    [SerializeField] private LayerMask lookLayers = ~0;

    private readonly System.Collections.Generic.List<IInteractable> activeInteractables = new System.Collections.Generic.List<IInteractable>();
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

    void Update()
    {
        if (activeInteractables.Count == 0)
        {
            return;
        }

        var nextInteractable = GetLookedAtInteractable() ?? GetClosestInteractable();
        if (nextInteractable != null && !Equals(nextInteractable, currentInteractable))
        {
            SetCurrentInteractable(nextInteractable);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var trigger = other.GetComponent<IInteractable>();
        if (trigger != null)
        {
            if (!activeInteractables.Contains(trigger))
            {
                activeInteractables.Add(trigger);
            }

            if (currentInteractable == null)
            {
                SetCurrentInteractable(trigger);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        var trigger = other.GetComponent<IInteractable>();
        if (trigger != null)
        {
            activeInteractables.Remove(trigger);

            if (trigger.Equals(currentInteractable))
            {
                ClearCurrentInteractable();
                var nextInteractable = GetClosestInteractable();
                if (nextInteractable != null)
                {
                    SetCurrentInteractable(nextInteractable);
                }
            }
        }
    }

    private void SetCurrentInteractable(IInteractable nextInteractable)
    {
        if (currentInteractable != null)
        {
            var previousInput = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
            previousInput.Action.action.performed -= OnInteract;
            previousInput.Action.action.Disable();
            currentInteractable.Deactive();
        }

        currentInteractable = nextInteractable;
        var input = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
        input.Action.action.performed += OnInteract;
        input.Action.action.Enable();

        var path = input.Action.action.bindings[0];
        currentInteractable.Active(path);
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable == null)
        {
            return;
        }

        var input = interactActions.FirstOrDefault(x => x.KeyActiveType == currentInteractable.keyType);
        input.Action.action.performed -= OnInteract;
        input.Action.action.Disable();
        currentInteractable.Deactive();

        currentInteractable = null;
    }

    private IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (var interactable in activeInteractables)
        {
            if (interactable == null)
            {
                continue;
            }

            var component = interactable as Component;
            if (component == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, component.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    private IInteractable GetLookedAtInteractable()
    {
        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            return null;
        }

        var ray = new Ray(cameraTarget.transform.position, cameraTarget.transform.forward);
        if (Physics.Raycast(ray, out var hit, lookRayDistance, lookLayers, QueryTriggerInteraction.Collide))
        {
            var interactable = hit.collider.GetComponent<IInteractable>() ?? hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && activeInteractables.Contains(interactable))
            {
                return interactable;
            }
        }

        return null;
    }
}

[Serializable]
public struct ActionToType
{
    public KeyActiveType KeyActiveType;
    public  InputActionProperty Action;
}
