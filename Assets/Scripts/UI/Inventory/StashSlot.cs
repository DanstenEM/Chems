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

    public enum SlotOwner
    {
        ExtractedInventory,
        Stash
    }

    [SerializeField] private Image image;
    [SerializeField] private SlotFilter filter = SlotFilter.Universal;
    [SerializeField] private SlotOwner owner = SlotOwner.Stash;

    public SlotOwner Owner => owner;

    public void Configure(Image slotImage, SlotFilter slotFilter = SlotFilter.Universal, SlotOwner slotOwner = SlotOwner.Stash)
    {
        image = slotImage;
        filter = slotFilter;
        owner = slotOwner;
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

    public static bool TryQuickTransferItem(StashMenuItem item)
    {
        if (item == null)
        {
            return false;
        }

        var sourceSlot = item.transform.parent != null
            ? item.transform.parent.GetComponent<StashSlot>()
            : null;
        if (sourceSlot == null)
        {
            return false;
        }

        var targetOwner = sourceSlot.Owner == SlotOwner.ExtractedInventory
            ? SlotOwner.Stash
            : SlotOwner.ExtractedInventory;

        var allSlots = Object.FindObjectsOfType<StashSlot>(true);

        // Prefer merging into an existing stack first.
        foreach (var candidate in allSlots)
        {
            if (candidate == null || candidate.Owner != targetOwner || !candidate.IsCategoryAllowed(item.Category))
            {
                continue;
            }

            var existingItem = candidate.GetComponentInChildren<StashMenuItem>();
            if (existingItem == null)
            {
                continue;
            }

            if (!string.Equals(existingItem.ItemId, item.ItemId, System.StringComparison.Ordinal))
            {
                continue;
            }

            existingItem.SetCount(existingItem.Count + item.Count);
            Object.Destroy(item.gameObject);
            return true;
        }

        // Otherwise move to first compatible empty slot.
        foreach (var candidate in allSlots)
        {
            if (candidate == null || candidate.Owner != targetOwner || !candidate.IsCategoryAllowed(item.Category))
            {
                continue;
            }

            if (candidate.GetComponentInChildren<StashMenuItem>() != null)
            {
                continue;
            }

            item.parentAfterDrag = candidate.transform;
            item.transform.SetParent(candidate.transform);
            item.SnapToParent();
            return true;
        }

        return false;
    }
}
