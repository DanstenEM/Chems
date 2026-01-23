using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            if(eventData.pointerDrag.TryGetComponent(out InventoryItem item))
            {
                item.parentAfterDrag = transform;
            }
        }
    }
}
