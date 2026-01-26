using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class HintCameraOrbit : IFixedTickable
{
    [Header("References")]
    public Transform target;
    public Transform myTransform;

    [Header("Orbit Settings")]
    public float radius = 0.5f;
    public float height = 1.6f;

    public HintCameraOrbit(Transform myTransform)
    {
        this.target = Camera.main.transform;
        this.myTransform = myTransform;
    }


    public void FixedTick()
    {
        myTransform.LookAt(target, target.up);
    }
}