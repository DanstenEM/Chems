using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InventorySystem : MonoBehaviour, IInitializable, IDisposable
{
    private InventorySlot[] slots;
    [SerializeField] private InventoryItem inventoryPrefab;
    [SerializeField] private InventoryItemObj inventoryObj;
    [SerializeField] private int selectSlot = -1;

    public InputActionProperty mouseAction;
    public Vector2 mousePosition;

    [Inject]
    public void Construct(InventorySlot[] slots)
    {
        this.slots = slots;
        mouseAction.action.Enable();
        mouseAction.action.performed += Action_performed;
        mouseAction.action.started += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        mousePosition = obj.ReadValue<Vector2>();
    }

    public void Initialize()
    {
        AddItem(inventoryObj);
        AddItem(inventoryObj);
        AddItem(inventoryObj);
        AddItem(inventoryObj);
        AddItem(inventoryObj);

        ChangeSelectSlot(1);
    }

    public bool AddItem(InventoryItemObj inventoryItemObj)
    {
        foreach (var item in slots)
        {
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
            var slotItem = item.GetComponentInChildren<InventoryItem>();
            if(slotItem == null)
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
        newItem.Construct(this,inventoryItemObj);
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
        mouseAction.action.Disable();
        mouseAction.action.performed -= Action_performed;
        mouseAction.action.started -= Action_performed;
    }

    public InventoryItemObj GetSelectedItem(bool use)
    {
        var slot = slots[selectSlot];
        var inventoryItem = slot.GetComponentInChildren<InventoryItem>();
        if(inventoryItem != null)
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
}
