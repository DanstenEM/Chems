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
                item.parentAfterDrag = transform;
                //inventoryItem = item;
            }
        }
    }

    public void Select() => image.color = selectColor;
    public void Deselect() => image.color = notSelectColor;
}
