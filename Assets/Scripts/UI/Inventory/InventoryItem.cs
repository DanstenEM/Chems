using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler
{
    [SerializeField] private Image image;
    [SerializeField] private InputActionProperty mouseAction;
    [SerializeField] private Vector2 mousePosition;
    public Transform parentAfterDrag;
    private InventorySlot[] slots;

    [Inject]
    public void Construct(InventorySlot[] inventorySlots)
    {
        slots = inventorySlots;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mouseAction.action.Enable();
        mouseAction.action.started += Action_performed;
        mouseAction.action.performed += Action_performed;

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);

        foreach (var item in slots)
        {
            item.NullItemToSlot(this);
        }
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        if(obj.performed)
            mousePosition = obj.ReadValue<Vector2>();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        mouseAction.action.Disable();
        mouseAction.action.started -= Action_performed;
        mouseAction.action.performed -= Action_performed;
        transform.SetParent(parentAfterDrag);

        if( parentAfterDrag.TryGetComponent(out InventorySlot component))
            component.inventoryItem = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = mousePosition;
    } 

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseAction.action.Enable();
        mousePosition = mouseAction.action.ReadValue<Vector2>();
        mouseAction.action.Disable();
    }
}
