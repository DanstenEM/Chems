using UnityEngine;
using Zenject;

public class PlayerObjContext : MonoInstaller
{
    [SerializeField] private Camera camTarget;
    [SerializeField] private ThirdPersonMovement thirdPerson;
    [SerializeField] private CMChangeView CMChangeView;

    public override void InstallBindings()
    {
        var cam = Camera.main;
        Container.Bind<Camera>().FromInstance(camTarget).AsSingle();

        Container.BindInterfacesAndSelfTo<ThirdPersonMovement>().FromInstance(thirdPerson).AsSingle();
        Container.BindInterfacesAndSelfTo<CMChangeView>().FromInstance(CMChangeView).AsSingle();
    }
}
