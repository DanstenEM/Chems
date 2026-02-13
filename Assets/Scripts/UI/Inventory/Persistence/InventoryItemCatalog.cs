using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemCatalog", menuName = "Inventory/Item Catalog")]
public class InventoryItemCatalog : ScriptableObject
{
    [SerializeField] private InventoryItemObj[] items;

    public InventoryItemObj[] Items => items;
}
