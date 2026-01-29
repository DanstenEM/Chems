using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Color selectColor, notSelectColor;
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            if(eventData.pointerDrag.TryGetComponent(out InventoryItem item))
            {
                if (!IsItemAllowed(item))
                {
                    return;
                }

                item.parentAfterDrag = transform;
                //inventoryItem = item;
            }
        }
    }

    public void Select() => image.color = selectColor;
    public void Deselect() => image.color = notSelectColor;

    public void Configure(Image targetImage, Color selectedColor, Color deselectedColor)
    {
        image = targetImage;
        selectColor = selectedColor;
        notSelectColor = deselectedColor;
        Deselect();
    }

    private bool IsItemAllowed(InventoryItem item)
    {
        if (item == null || item.itemObj == null)
        {
            return false;
        }

        var marker = GetComponent<InventorySlotMarker>();
        var slotCategory = marker != null ? marker.Category : InventorySlotMarker.SlotCategory.Regular;

        return slotCategory switch
        {
            InventorySlotMarker.SlotCategory.Chemical => item.itemObj.category == InventoryItemObj.ItemCategory.Chemical,
            InventorySlotMarker.SlotCategory.Weapon => item.itemObj.category == InventoryItemObj.ItemCategory.Weapon,
            _ => item.itemObj.category == InventoryItemObj.ItemCategory.Regular
        };
    }
}
