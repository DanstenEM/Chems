using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemObj", menuName = "Scriptable Objects/InventoryItemObj")]
public class InventoryItemObj : ScriptableObject
{
    public int stackCount;
    public Sprite icon;

    public bool isStackable;
}
