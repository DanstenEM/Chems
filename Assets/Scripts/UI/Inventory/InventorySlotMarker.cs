using UnityEngine;

public class InventorySlotMarker : MonoBehaviour
{
    public enum SlotCategory
    {
        Regular,
        Chemical,
        Weapon,
        Any
    }

    [field: SerializeField] public SlotCategory Category { get; private set; }
    [field: SerializeField] public int Index { get; private set; }

    public void Setup(string groupName, int index)
    {
        Index = index;
        Category = groupName switch
        {
            "RegularSlots" => SlotCategory.Regular,
            "ChemicalSlots" => SlotCategory.Chemical,
            "WeaponSlots" => SlotCategory.Weapon,
            "LootSlots" => SlotCategory.Any,
            _ => SlotCategory.Regular
        };
    }
}
