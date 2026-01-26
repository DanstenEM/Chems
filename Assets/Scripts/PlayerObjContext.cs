using UnityEngine;
using Zenject;

public class PlayerObjContext : MonoInstaller
{
    [SerializeField] private Camera camTarget;
    [SerializeField] private ThirdPersonMovement thirdPerson;
    public override void InstallBindings()
    {
        var cam = Camera.main;
        Container.Bind<Camera>().FromInstance(camTarget).AsSingle();

        Container.BindInterfacesAndSelfTo<ThirdPersonMovement>().FromInstance(thirdPerson).AsSingle();
    }
}
