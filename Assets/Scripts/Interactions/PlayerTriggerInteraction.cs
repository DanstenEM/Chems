using Assets.Scripts.Interactions.Abstract;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTriggerInteraction : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private ActionToType[] interactActions;
    [SerializeField] private float lookRayDistance = 4f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private IInteractable currentInteractable;

    void OnDestroy()
    {
        ClearCurrentInteractable();
        foreach (var item in interactActions)
        {
            item.Action.action.performed -= OnInteract;
        }
    }

    private void Update()
    {
        UpdateLookedInteractable();
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void UpdateLookedInteractable()
    {
        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            ClearCurrentInteractable();
            return;
        }

        if (Physics.Raycast(cameraTarget.transform.position, cameraTarget.transform.forward, out var hit, lookRayDistance, interactableLayers))
        {
            var lookedInteractable = hit.collider.GetComponentInParent<IInteractable>();
            if (lookedInteractable != null)
            {
                if (!ReferenceEquals(currentInteractable, lookedInteractable))
                {
                    SetCurrentInteractable(lookedInteractable);
                }
                return;
            }
        }

        ClearCurrentInteractable();
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        ClearCurrentInteractable();
        currentInteractable = interactable;

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
}

[Serializable]
public struct ActionToType
{
    public KeyActiveType KeyActiveType;
    public  InputActionProperty Action;
}
