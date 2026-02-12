using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InventorySystem : MonoBehaviour, IInitializable, IDisposable
{
    private static InventorySystem gameplayInventory;
    private static readonly List<InventorySystem> registeredInventories = new List<InventorySystem>();

    [SerializeField] private InventorySlot[] slots;
    [SerializeField] private InventoryItem inventoryPrefab;
    [SerializeField] private InventoryItemObj inventoryObj;
    [SerializeField] private int selectSlot = -1;
    [SerializeField] private bool addStarterItems;
    [SerializeField] private int starterItemCount = 0;
    [SerializeField] private bool isGameplayInventory = true;
    [SerializeField] private bool restoreFromPostExtractionOnInitialize = true;
    [SerializeField] private float dropForwardDistance = 1.5f;
    [SerializeField] private float dropHeightOffset = 0.4f;
    [SerializeField] private string playerTag = "Player";

    public InputActionProperty mouseAction;
    public Vector2 mousePosition;
    private bool mouseActionBound;

    public static InventorySystem GameplayInventory
    {
        get
        {
            if (gameplayInventory != null)
            {
                return gameplayInventory;
            }

            gameplayInventory = FindGameplayInventoryInScene();
            return gameplayInventory;
        }
    }

    [Inject]
    public void Construct(InventorySlot[] slots)
    {
        this.slots = slots;
        BindMouseAction();
    }

    private void Awake()
    {
        if (!registeredInventories.Contains(this))
        {
            registeredInventories.Add(this);
        }

        if (isGameplayInventory)
        {
            gameplayInventory = this;
        }

        if (slots == null || slots.Length == 0)
        {
            SetSlots(FindObjectsOfType<InventorySlot>());
        }

        BindMouseAction();
    }

    private void OnDestroy()
    {
        registeredInventories.Remove(this);

        if (ReferenceEquals(gameplayInventory, this))
        {
            gameplayInventory = null;
        }
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        mousePosition = obj.ReadValue<Vector2>();
    }

    public void Initialize()
    {
        bool restoredFromPostExtraction = TryRestoreFromPostExtraction();

        if (!restoredFromPostExtraction && addStarterItems && inventoryObj != null)
        {
            for (int i = 0; i < starterItemCount; i++)
            {
                AddItem(inventoryObj);
            }
        }

        if (slots != null && slots.Length > 0)
        {
            if (!SelectWeaponSlot(0))
            {
                ChangeSelectSlot(0);
            }
        }
    }

    private bool TryRestoreFromPostExtraction()
    {
        if (!isGameplayInventory || !restoreFromPostExtractionOnInitialize)
        {
            return false;
        }

        if (!InventoryPersistenceService.HasPostExtractionInventorySave())
        {
            return false;
        }

        SavedInventory snapshot = InventoryPersistenceService.LoadPostExtractionInventory();
        IReadOnlyDictionary<string, InventoryItemObj> lookup = InventorySnapshotMapper.BuildLookupFromResources();
        InventorySnapshotMapper.RestoreSnapshot(this, snapshot, lookup, true);
        return true;
    }

    public bool AddItem(InventoryItemObj inventoryItemObj)
    {
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        bool useRegularOnly = inventoryItemObj != null && inventoryItemObj.isDefaultItem;

        if (TryAddToExistingStack(inventoryItemObj, useRegularOnly))
        {
            return true;
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

    private bool TryAddToExistingStack(InventoryItemObj inventoryItemObj, bool useRegularOnly)
    {
        if (inventoryItemObj != null && inventoryItemObj.category == InventoryItemObj.ItemCategory.Weapon)
        {
            return false;
        }

        foreach (var slot in slots)
        {
            if (!IsSlotCompatible(slot, inventoryItemObj, useRegularOnly))
            {
                continue;
            }

            var slotItem = slot.GetComponentInChildren<InventoryItem>();
            if (slotItem == null || slotItem.itemObj != inventoryItemObj)
            {
                continue;
            }

            slotItem.count += 1;
            slotItem.RefrashCount();
            return true;
        }

        return false;
    }


    public static InventorySystem FindInventoryBySlot(InventorySlot slot)
    {
        if (slot == null)
        {
            return null;
        }

        foreach (var inventory in registeredInventories)
        {
            if (inventory == null || inventory.slots == null)
            {
                continue;
            }

            foreach (var inventorySlot in inventory.slots)
            {
                if (ReferenceEquals(inventorySlot, slot))
                {
                    return inventory;
                }
            }
        }

        return null;
    }

    public bool TryQuickTransferItem(InventoryItem item)
    {
        if (item == null || item.itemObj == null)
        {
            return false;
        }

        var targetInventory = FindQuickTransferTarget(item.itemObj);
        if (targetInventory == null)
        {
            return false;
        }

        int originalCount = item.count;
        int movedCount = 0;

        for (int i = 0; i < originalCount; i++)
        {
            if (!targetInventory.AddItem(item.itemObj))
            {
                break;
            }

            movedCount++;
        }

        if (movedCount == 0)
        {
            return false;
        }

        item.count -= movedCount;
        if (item.count <= 0)
        {
            Destroy(item.gameObject);
        }
        else
        {
            item.RefrashCount();
        }

        return true;
    }

    public void SpawnNewItem(InventoryItemObj inventoryItemObj, InventorySlot inventorySlot)
    {
        var newItem = Instantiate(inventoryPrefab, inventorySlot.transform);
        newItem.Construct(this, inventoryItemObj);
    }

    public bool TryDropDraggedItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.itemObj == null)
        {
            return false;
        }

        if (inventoryItem.itemObj.dropPrefab == null)
        {
            return false;
        }

        Transform dropOrigin = GetDropOrigin();
        if (dropOrigin == null)
        {
            return false;
        }

        Vector3 forward = dropOrigin.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        Vector3 spawnPosition = dropOrigin.position + forward.normalized * dropForwardDistance + Vector3.up * dropHeightOffset;
        Instantiate(inventoryItem.itemObj.dropPrefab, spawnPosition, Quaternion.identity);

        if (inventoryItem.count > 1)
        {
            inventoryItem.count -= 1;
            inventoryItem.RefrashCount();
        }
        else
        {
            Destroy(inventoryItem.gameObject);
        }

        return true;
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
        if (slots == null || slots.Length == 0 || selectSlot < 0 || selectSlot >= slots.Length)
        {
            return null;
        }

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

    public InventorySlot[] GetSlots()
    {
        return slots;
    }

    public int GetSelectedSlotIndex()
    {
        return selectSlot;
    }

    public bool HasSelectedWeapon()
    {
        var item = GetSelectedItem(false);
        return item != null && item.category == InventoryItemObj.ItemCategory.Weapon;
    }

    public bool SelectWeaponSlot(int weaponIndex)
    {
        if (weaponIndex < 0 || slots == null || slots.Length == 0)
        {
            return false;
        }

        var weaponSlots = new List<InventorySlot>();
        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            var marker = slot.GetComponent<InventorySlotMarker>();
            if (marker != null && marker.Category == InventorySlotMarker.SlotCategory.Weapon)
            {
                weaponSlots.Add(slot);
            }
        }

        weaponSlots.Sort((left, right) =>
        {
            var leftMarker = left.GetComponent<InventorySlotMarker>();
            var rightMarker = right.GetComponent<InventorySlotMarker>();
            var leftIndex = leftMarker != null ? leftMarker.Index : int.MaxValue;
            var rightIndex = rightMarker != null ? rightMarker.Index : int.MaxValue;
            return leftIndex.CompareTo(rightIndex);
        });

        if (weaponIndex >= weaponSlots.Count)
        {
            return false;
        }

        var weaponSlot = weaponSlots[weaponIndex];
        var slotIndex = Array.IndexOf(slots, weaponSlot);
        if (slotIndex < 0)
        {
            return false;
        }

        ChangeSelectSlot(slotIndex);
        return true;
    }

    private Transform GetDropOrigin()
    {
        if (!string.IsNullOrWhiteSpace(playerTag))
        {
            var taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
            if (taggedPlayer != null)
            {
                return taggedPlayer.transform;
            }
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return transform;
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
            return slotCategory == InventorySlotMarker.SlotCategory.Regular ||
                   slotCategory == InventorySlotMarker.SlotCategory.Universal;
        }

        if (itemObj == null)
        {
            return true;
        }

        return slotCategory switch
        {
            InventorySlotMarker.SlotCategory.Universal => true,
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

    private static InventorySystem FindGameplayInventoryInScene()
    {
        var inventorySystems = FindObjectsOfType<InventorySystem>();
        foreach (var inventorySystem in inventorySystems)
        {
            if (inventorySystem != null && inventorySystem.isGameplayInventory)
            {
                return inventorySystem;
            }
        }

        return null;
    }

    private InventorySystem FindQuickTransferTarget(InventoryItemObj itemObj)
    {
        InventorySystem fallbackTarget = null;

        foreach (var candidate in registeredInventories)
        {
            if (candidate == null || candidate == this)
            {
                continue;
            }

            if (!candidate.CanReceiveForQuickTransfer(itemObj))
            {
                continue;
            }

            if (!isGameplayInventory && candidate.isGameplayInventory)
            {
                return candidate;
            }

            fallbackTarget ??= candidate;
        }

        return fallbackTarget;
    }

    private bool CanReceiveForQuickTransfer(InventoryItemObj itemObj)
    {
        if (!isActiveAndEnabled || slots == null || slots.Length == 0)
        {
            return false;
        }

        foreach (var slot in slots)
        {
            if (slot == null || !slot.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (slot.IsItemAllowed(itemObj))
            {
                return true;
            }
        }

        return false;
    }
}
