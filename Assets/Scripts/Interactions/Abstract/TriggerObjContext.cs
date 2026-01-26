using TMPro;
using UnityEngine;
using Zenject;

public class TriggerObjContext : MonoInstaller
{
    [SerializeField] private Transform parent;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<HintCameraOrbit>().FromNew().AsSingle().WithArguments(parent);
    }
}
