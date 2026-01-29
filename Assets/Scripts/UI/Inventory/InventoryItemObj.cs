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

    public int stackCount;
    public Sprite icon;

    public bool isStackable;
    public bool isDefaultItem = true;
    public ItemCategory category = ItemCategory.Regular;
}
