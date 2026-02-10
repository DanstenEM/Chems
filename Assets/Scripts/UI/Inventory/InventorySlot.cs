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
        return item != null && IsItemAllowed(item.itemObj);
    }

    public bool IsItemAllowed(InventoryItemObj itemObj)
    {
        if (itemObj == null)
        {
            return false;
        }

        var marker = GetComponent<InventorySlotMarker>();
        var slotCategory = marker != null ? marker.Category : InventorySlotMarker.SlotCategory.Regular;

        return slotCategory switch
        {
            InventorySlotMarker.SlotCategory.Universal => true,
            InventorySlotMarker.SlotCategory.Chemical => itemObj.category == InventoryItemObj.ItemCategory.Chemical,
            InventorySlotMarker.SlotCategory.Weapon => itemObj.category == InventoryItemObj.ItemCategory.Weapon,
            _ => itemObj.category == InventoryItemObj.ItemCategory.Regular
        };
    }
}
