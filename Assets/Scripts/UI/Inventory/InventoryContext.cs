using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InventoryContext : MonoInstaller
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private InventorySlot[] slots;
    public override void InstallBindings()
    {
        Container.Bind<InventorySlot[]>().FromInstance(slots).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InventorySystem>().FromInstance(inventorySystem).AsSingle().NonLazy();
    }
}
