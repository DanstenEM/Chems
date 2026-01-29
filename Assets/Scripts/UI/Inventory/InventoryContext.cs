using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InventoryContext : MonoInstaller
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private InventoryLayout inventoryLayout;
    [SerializeField] private InventorySlot[] slots;
    public override void InstallBindings()
    {
        var resolvedSlots = slots;
        if ((resolvedSlots == null || resolvedSlots.Length == 0) && inventoryLayout != null)
        {
            resolvedSlots = inventoryLayout.BuildLayout();
        }

        if (resolvedSlots == null || resolvedSlots.Length == 0)
        {
            resolvedSlots = inventorySystem.GetComponentsInChildren<InventorySlot>(true);
        }

        Container.Bind<InventorySlot[]>().FromInstance(resolvedSlots).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InventorySystem>().FromInstance(inventorySystem).AsSingle().NonLazy();
    }
}
