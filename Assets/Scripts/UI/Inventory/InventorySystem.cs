using UnityEngine;
using Zenject;

public class InventorySystem : MonoBehaviour, IInitializable
{
    private InventorySlot[] slots;

    [Inject]
    public void Construct(InventorySlot[] slots)
    {
        this.slots = slots;
    }
    public void Initialize()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Construct(i);
        }
    }
}
