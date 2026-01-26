using UnityEngine;
using Zenject;

public class InventoryContext : MonoInstaller
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private InventorySlot[] slots;
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<InventorySystem>().FromInstance(inventorySystem).AsSingle();
        Container.Bind<InventorySlot[]>().FromInstance(slots).AsSingle().NonLazy();
    }
}
