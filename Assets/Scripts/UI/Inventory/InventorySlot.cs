using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Color selectColor, notSelectColor;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || !eventData.pointerDrag.TryGetComponent(out InventoryItem draggedItem))
        {
            return;
        }

        if (!IsItemAllowed(draggedItem))
        {
            return;
        }

        var targetItem = GetComponentInChildren<InventoryItem>();
        if (targetItem == null)
        {
            draggedItem.parentAfterDrag = transform;
            return;
        }

        if (!CanStackTogether(draggedItem, targetItem))
        {
            return;
        }

        int stackCapacity = Mathf.Max(1, targetItem.itemObj.stackCount);
        int freeSpace = stackCapacity - targetItem.count;
        if (freeSpace <= 0)
        {
            return;
        }

        int moveCount = Mathf.Min(draggedItem.count, freeSpace);
        targetItem.count += moveCount;
        targetItem.RefrashCount();

        draggedItem.count -= moveCount;
        if (draggedItem.count <= 0)
        {
            Destroy(draggedItem.gameObject);
        }
        else
        {
            draggedItem.RefrashCount();
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

    private static bool CanStackTogether(InventoryItem draggedItem, InventoryItem targetItem)
    {
        if (draggedItem == null || targetItem == null || draggedItem.itemObj == null || targetItem.itemObj == null)
        {
            return false;
        }

        return targetItem.itemObj.isStackable && draggedItem.itemObj == targetItem.itemObj;
    }
}
