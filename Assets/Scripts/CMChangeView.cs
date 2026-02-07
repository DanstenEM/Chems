using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Zenject;
using System;

public class CMChangeView : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private Vector3 right;
    [SerializeField] private Vector3 left;
    [SerializeField] private Transform target;
    [SerializeField] private float duration;
    [SerializeField] private bool isRight = true;

    [SerializeField] private Health Health;
    private bool isDie;

    [SerializeField] private InputActionProperty actionChange;

    private void Action_performedR(InputAction.CallbackContext obj)
    {
        if (isDie || LootCrateUI.IsAnyLootMenuOpen) return;

        isRight = !isRight;
        ChangeView(isRight);
    }
    
    private void ChangeView(bool value)
    {
        if (isDie || LootCrateUI.IsAnyLootMenuOpen) return;

        var tweeen = value switch
        {
            true => target.transform.DOLocalMove(right, duration),
            false => target.transform.DOLocalMove(left, duration)
        };
        tweeen.Play();
    }

    public void Initialize()
    {
        Health.isDie.Changed += IsDie_Changed;
        actionChange.action.Enable();
        actionChange.action.performed += Action_performedR;

        ChangeView(true);
    }

    private void IsDie_Changed(bool obj)
    {
        isDie = obj;
    }

    public void Dispose()
    {
        Health.isDie.Changed -= IsDie_Changed;
        actionChange.action.Disable();
        actionChange.action.performed -= Action_performedR;
    }
}
