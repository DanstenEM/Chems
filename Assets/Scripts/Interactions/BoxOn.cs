using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class BoxOn : TriggerInteractable
{
    public GameObject target;
    bool isOn = false;

    public override void Active(InputBinding input)
    {
        base.Active(input);
    }

    public override void Deactive()
    {
        base.Deactive();
    }

    public override void Interact()
    {
        if (isOn)
        {
            isOn = false;
            target.SetActive(false);
        }
        else
        {
            isOn = true;
            target.SetActive(true);
        }
    }
}
