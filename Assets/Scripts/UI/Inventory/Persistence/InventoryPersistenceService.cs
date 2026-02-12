using System;
using System.IO;
using UnityEngine;

public static class InventoryPersistenceService
{
    private const string DefaultStashKey = "stash";
    private const string DefaultPostExtractionKey = "post_extract_inventory";

    public static bool SaveStash(SavedInventory inventory)
    {
        return Save(DefaultStashKey, inventory);
    }

    public static SavedInventory LoadStash()
    {
        return Load(DefaultStashKey);
    }

    public static bool SavePostExtractionInventory(SavedInventory inventory)
    {
        return Save(DefaultPostExtractionKey, inventory);
    }

    public static SavedInventory LoadPostExtractionInventory()
    {
        return Load(DefaultPostExtractionKey);
    }

    public static void ClearPostExtractionInventory()
    {
        Delete(DefaultPostExtractionKey);
    }

    public static void ClearStash()
    {
        Delete(DefaultStashKey);
    }

    public static bool Save(string key, SavedInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError("Cannot save null inventory.");
            return false;
        }

        try
        {
            string path = GetSavePath(key);
            var wrapper = new SavedInventoryWrapper
            {
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                inventory = inventory
            };

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save inventory: {exception.Message}");
            return false;
        }
    }

    public static SavedInventory Load(string key)
    {
        try
        {
            string path = GetSavePath(key);
            if (!File.Exists(path))
            {
                return new SavedInventory();
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new SavedInventory();
            }

            var wrapper = JsonUtility.FromJson<SavedInventoryWrapper>(json);
            return wrapper != null && wrapper.inventory != null
                ? wrapper.inventory
                : new SavedInventory();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to load inventory: {exception.Message}");
            return new SavedInventory();
        }
    }

    public static void Delete(string key)
    {
        try
        {
            string path = GetSavePath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to delete inventory save: {exception.Message}");
        }
    }

    private static string GetSavePath(string key)
    {
        string sanitizedKey = string.IsNullOrWhiteSpace(key) ? DefaultStashKey : key.Trim();
        return Path.Combine(Application.persistentDataPath, $"{sanitizedKey}_inventory.json");
    }

    [Serializable]
    private class SavedInventoryWrapper
    {
        public string savedAtUtc;
        public SavedInventory inventory;
    }
}
