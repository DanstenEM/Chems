using System.Collections.Generic;
using UnityEngine;

public static class InventorySnapshotMapper
{
    public static SavedInventory BuildSnapshot(InventorySystem inventorySystem)
    {
        var snapshot = new SavedInventory();
        if (inventorySystem == null)
        {
            return snapshot;
        }

        var stacksByItemId = new Dictionary<string, int>();
        var slots = inventorySystem.GetSlots();
        if (slots == null)
        {
            return snapshot;
        }

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item == null || item.itemObj == null)
            {
                continue;
            }

            string itemId = item.itemObj.ItemId;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            int count = Mathf.Max(0, item.count);
            if (count <= 0)
            {
                continue;
            }

            if (!stacksByItemId.TryAdd(itemId, count))
            {
                stacksByItemId[itemId] += count;
            }
        }

        foreach (var pair in stacksByItemId)
        {
            snapshot.stacks.Add(new SavedItemStack
            {
                itemId = pair.Key,
                count = pair.Value
            });
        }

        return snapshot;
    }

    public static SavedInventory MergeSnapshots(SavedInventory baseSnapshot, SavedInventory addedSnapshot)
    {
        var merged = new SavedInventory();
        var totalByItemId = new Dictionary<string, int>();

        AddSnapshotToTotals(baseSnapshot, totalByItemId);
        AddSnapshotToTotals(addedSnapshot, totalByItemId);

        foreach (var pair in totalByItemId)
        {
            merged.stacks.Add(new SavedItemStack
            {
                itemId = pair.Key,
                count = pair.Value
            });
        }

        return merged;
    }

    public static int RestoreSnapshot(InventorySystem inventorySystem, SavedInventory snapshot, IReadOnlyDictionary<string, InventoryItemObj> itemLookup, bool clearBeforeRestore = true)
    {
        if (inventorySystem == null || snapshot == null || itemLookup == null)
        {
            return 0;
        }

        if (clearBeforeRestore)
        {
            ClearInventory(inventorySystem);
        }

        int restoredItemsCount = 0;

        foreach (var stack in snapshot.stacks)
        {
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.count <= 0)
            {
                continue;
            }

            if (!itemLookup.TryGetValue(stack.itemId, out var itemObj) || itemObj == null)
            {
                Debug.LogWarning($"Could not restore item '{stack.itemId}' because it is missing in item lookup.");
                continue;
            }

            for (int i = 0; i < stack.count; i++)
            {
                if (!inventorySystem.AddItem(itemObj))
                {
                    return restoredItemsCount;
                }

                restoredItemsCount++;
            }
        }

        return restoredItemsCount;
    }

    public static IReadOnlyDictionary<string, InventoryItemObj> BuildLookupFromResources()
    {
        var lookup = new Dictionary<string, InventoryItemObj>();
        var allItems = Resources.LoadAll<InventoryItemObj>(string.Empty);

        foreach (var itemObj in allItems)
        {
            if (itemObj == null)
            {
                continue;
            }

            string itemId = itemObj.ItemId;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            if (!lookup.TryAdd(itemId, itemObj))
            {
                Debug.LogWarning($"Duplicate inventory item id '{itemId}' found. Keeping first occurrence.");
            }
        }

        return lookup;
    }

    private static void AddSnapshotToTotals(SavedInventory snapshot, IDictionary<string, int> totals)
    {
        if (snapshot == null || snapshot.stacks == null)
        {
            return;
        }

        foreach (var stack in snapshot.stacks)
        {
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId))
            {
                continue;
            }

            int count = Mathf.Max(0, stack.count);
            if (count <= 0)
            {
                continue;
            }

            if (!totals.TryAdd(stack.itemId, count))
            {
                totals[stack.itemId] += count;
            }
        }
    }


    public static List<InventoryItemObj> BuildItemList(SavedInventory snapshot, IReadOnlyDictionary<string, InventoryItemObj> itemLookup)
    {
        var items = new List<InventoryItemObj>();
        if (snapshot == null || snapshot.stacks == null || itemLookup == null)
        {
            return items;
        }

        foreach (var stack in snapshot.stacks)
        {
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.count <= 0)
            {
                continue;
            }

            if (!itemLookup.TryGetValue(stack.itemId, out var itemObj) || itemObj == null)
            {
                Debug.LogWarning($"Could not map item '{stack.itemId}' to InventoryItemObj.");
                continue;
            }

            for (int i = 0; i < stack.count; i++)
            {
                items.Add(itemObj);
            }
        }

        return items;
    }

    public static void ClearInventoryContents(InventorySystem inventorySystem)
    {
        if (inventorySystem == null)
        {
            return;
        }

        ClearInventory(inventorySystem);
    }

    private static void ClearInventory(InventorySystem inventorySystem)
    {
        var slots = inventorySystem.GetSlots();
        if (slots == null)
        {
            return;
        }

        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(slot.transform.GetChild(i).gameObject);
            }
        }
    }
}
