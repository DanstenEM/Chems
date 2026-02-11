using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StashSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image image;

    public void Configure(Image slotImage)
    {
        image = slotImage;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!eventData.pointerDrag.TryGetComponent(out StashMenuItem draggedItem))
        {
            return;
        }

        var existingItem = GetComponentInChildren<StashMenuItem>();
        if (existingItem == null)
        {
            draggedItem.parentAfterDrag = transform;
            return;
        }

        if (ReferenceEquals(existingItem, draggedItem))
        {
            return;
        }

        if (!string.Equals(existingItem.ItemId, draggedItem.ItemId, System.StringComparison.Ordinal))
        {
            return;
        }

        existingItem.SetCount(existingItem.Count + draggedItem.Count);
        Destroy(draggedItem.gameObject);
    }
}
