using UnityEngine;
using UnityEngine.InputSystem;

public class BoxOn : TriggerInteractable
{
    public GameObject UI;
    public GameObject target;
    bool isOn = false;

    public override void Active(InputBinding input)
    {
        base.Active(input);

        UI.SetActive(true);
    }

    public override void Deactive()
    {
        UI.SetActive(false);
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
