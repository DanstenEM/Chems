using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InventorySystem : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private InventorySlot[] slots;
    [SerializeField] private InventoryItem inventoryPrefab;
    [SerializeField] private InventoryItemObj inventoryObj;
    [SerializeField] private int selectSlot = -1;
    [SerializeField] private bool addStarterItems;
    [SerializeField] private int starterItemCount = 0;

    public InputActionProperty mouseAction;
    public Vector2 mousePosition;
    private bool mouseActionBound;

    [Inject]
    public void Construct(InventorySlot[] slots)
    {
        this.slots = slots;
        BindMouseAction();
    }

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            SetSlots(FindObjectsOfType<InventorySlot>());
        }

        BindMouseAction();
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        mousePosition = obj.ReadValue<Vector2>();
    }

    public void Initialize()
    {
        if (addStarterItems && inventoryObj != null)
        {
            for (int i = 0; i < starterItemCount; i++)
            {
                AddItem(inventoryObj);
            }
        }

        if (slots != null && slots.Length > 0)
        {
            ChangeSelectSlot(0);
        }
    }

    public bool AddItem(InventoryItemObj inventoryItemObj)
    {
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        bool useRegularOnly = inventoryItemObj != null && inventoryItemObj.isDefaultItem;

        foreach (var item in slots)
        {
            if (!IsSlotCompatible(item, inventoryItemObj, useRegularOnly))
            {
                continue;
            }

            var slotItem = item.GetComponentInChildren<InventoryItem>();
            if (slotItem != null && slotItem.itemObj == inventoryItemObj &&
                slotItem.count < slotItem.itemObj.stackCount
                && slotItem.itemObj.isStackable == true)
            {
                slotItem.count += 1;
                slotItem.RefrashCount();
                return true;
            }
        }

        foreach (var item in slots)
        {
            if (!IsSlotCompatible(item, inventoryItemObj, useRegularOnly))
            {
                continue;
            }

            var slotItem = item.GetComponentInChildren<InventoryItem>();
            if (slotItem == null)
            {
                SpawnNewItem(inventoryItemObj, item);
                return true;
            }
        }

        return false;
    }

    public void SpawnNewItem(InventoryItemObj inventoryItemObj, InventorySlot inventorySlot)
    {
        var newItem = Instantiate(inventoryPrefab, inventorySlot.transform);
        newItem.Construct(this, inventoryItemObj);
    }

    public void ChangeSelectSlot(int newValue)
    {
        if (selectSlot > 0)
            slots[selectSlot].Deselect();

        slots[newValue].Select();
        selectSlot = newValue;
    }

    public void Dispose()
    {
        if (!mouseActionBound || mouseAction.action == null)
        {
            return;
        }

        mouseAction.action.Disable();
        mouseAction.action.performed -= Action_performed;
        mouseAction.action.started -= Action_performed;
        mouseActionBound = false;
    }

    public InventoryItemObj GetSelectedItem(bool use)
    {
        var slot = slots[selectSlot];
        var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
        if (inventoryItem != null)
        {
            var item = inventoryItem.itemObj;
            if (use)
            {
                inventoryItem.count--;
                if (inventoryItem.count <= 0)
                    Destroy(inventoryItem);
                else
                    inventoryItem.RefrashCount();
            }

            return item;
        }

        return null;
    }

    private static bool IsRegularSlot(InventorySlot slot)
    {
        var marker = slot.GetComponent<InventorySlotMarker>();
        return marker == null || marker.Category == InventorySlotMarker.SlotCategory.Regular;
    }

    private static bool IsSlotCompatible(InventorySlot slot, InventoryItemObj itemObj, bool useRegularOnly)
    {
        if (slot == null)
        {
            return false;
        }

        var marker = slot.GetComponent<InventorySlotMarker>();
        var slotCategory = marker != null ? marker.Category : InventorySlotMarker.SlotCategory.Regular;

        if (useRegularOnly)
        {
            return slotCategory == InventorySlotMarker.SlotCategory.Regular;
        }

        if (itemObj == null)
        {
            return true;
        }

        return slotCategory switch
        {
            InventorySlotMarker.SlotCategory.Chemical => itemObj.category == InventoryItemObj.ItemCategory.Chemical,
            InventorySlotMarker.SlotCategory.Weapon => itemObj.category == InventoryItemObj.ItemCategory.Weapon,
            _ => itemObj.category == InventoryItemObj.ItemCategory.Regular
        };
    }

    public void SetSlots(InventorySlot[] newSlots)
    {
        if (newSlots == null || newSlots.Length == 0)
        {
            return;
        }

        Array.Sort(newSlots, CompareSlots);
        slots = newSlots;
    }

    private static int CompareSlots(InventorySlot left, InventorySlot right)
    {
        var leftMarker = left.GetComponent<InventorySlotMarker>();
        var rightMarker = right.GetComponent<InventorySlotMarker>();

        int leftCategory = leftMarker != null ? (int)leftMarker.Category : int.MaxValue;
        int rightCategory = rightMarker != null ? (int)rightMarker.Category : int.MaxValue;
        int categoryCompare = leftCategory.CompareTo(rightCategory);
        if (categoryCompare != 0)
        {
            return categoryCompare;
        }

        int leftIndex = leftMarker != null ? leftMarker.Index : int.MaxValue;
        int rightIndex = rightMarker != null ? rightMarker.Index : int.MaxValue;
        return leftIndex.CompareTo(rightIndex);
    }

    private void BindMouseAction()
    {
        if (mouseActionBound || mouseAction.action == null)
        {
            return;
        }

        mouseAction.action.Enable();
        mouseAction.action.performed += Action_performed;
        mouseAction.action.started += Action_performed;
        mouseActionBound = true;
    }
}
