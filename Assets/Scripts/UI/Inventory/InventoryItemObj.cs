using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemObj", menuName = "Scriptable Objects/InventoryItemObj")]
public class InventoryItemObj : ScriptableObject
{
    public enum ItemCategory
    {
        Regular,
        Chemical,
        Weapon
    }

    [SerializeField] private string itemId;

    public int stackCount;
    public Sprite icon;

    public bool isStackable;
    public bool isDefaultItem = true;
    public ItemCategory category = ItemCategory.Regular;
    public GameObject dropPrefab;

    public string ItemId => string.IsNullOrWhiteSpace(itemId) ? name : itemId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        itemId = UnityEditor.GUID.Generate().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
