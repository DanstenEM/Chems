using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [field: SerializeField] public InventoryItemObj itemObj { get; private set; }
    [field: SerializeField] public int count { get; set; } = 1;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text counter;
    private InventorySystem inventorySystem;
    
    public Transform parentAfterDrag;

    public void Construct(InventorySystem inventorySystem,InventoryItemObj inventoryItemObj)
    {
        itemObj = inventoryItemObj;
        this.inventorySystem = inventorySystem;
        if (image != null)
        {
            image.color = new Color(1f, 0.85f, 0.2f, 1f);
            if (itemObj != null && itemObj.icon != null)
            {
                image.sprite = itemObj.icon;
                image.preserveAspect = true;
            }
        }
        RefrashCount();
    }

    public void RefrashCount()
    {
        counter.text = count.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);

        //if( parentAfterDrag.TryGetComponent(out InventorySlot component))
        //    component.inventoryItem = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = inventorySystem.mousePosition;
    } 
}
