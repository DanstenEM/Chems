using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int slotIndex;
    public InventoryItem inventoryItem = null;
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            if(eventData.pointerDrag.TryGetComponent(out InventoryItem item))
            {
                item.parentAfterDrag = transform;
                inventoryItem = item;
            }
        }
    }
    
    public void Construct(int indexSlot)
    {
        slotIndex = indexSlot;

        if(transform.childCount > 0)
        {
            var item = transform.GetComponentInChildren<InventoryItem>();

            if (item is InventoryItem itemSlot)
                inventoryItem = itemSlot;
        }
    }

    public void NullItemToSlot(InventoryItem inventoryItem)
    {
        if (this.inventoryItem != null && this.inventoryItem.Equals(inventoryItem))
            this.inventoryItem = null;
    }
}
