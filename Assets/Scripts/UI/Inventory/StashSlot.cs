using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StashSlot : MonoBehaviour, IDropHandler
{
    public enum SlotFilter
    {
        Universal,
        Regular,
        Chemical,
        Weapon
    }

    [SerializeField] private Image image;
    [SerializeField] private SlotFilter filter = SlotFilter.Universal;

    public void Configure(Image slotImage, SlotFilter slotFilter = SlotFilter.Universal)
    {
        image = slotImage;
        filter = slotFilter;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!eventData.pointerDrag.TryGetComponent(out StashMenuItem draggedItem))
        {
            return;
        }

        if (!IsCategoryAllowed(draggedItem.Category))
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

    public bool IsCategoryAllowed(InventoryItemObj.ItemCategory category)
    {
        return filter switch
        {
            SlotFilter.Universal => true,
            SlotFilter.Chemical => category == InventoryItemObj.ItemCategory.Chemical,
            SlotFilter.Weapon => category == InventoryItemObj.ItemCategory.Weapon,
            _ => category == InventoryItemObj.ItemCategory.Regular
        };
    }
}
