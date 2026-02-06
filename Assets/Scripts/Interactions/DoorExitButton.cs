using UnityEngine;

public class DoorExitButton : TriggerInteractable
{
    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private bool allowRepeatedUse;

    private bool hasOpened;

    private void Awake()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponentInParent<Animator>();
        }
    }

    public override void Interact()
    {
        if (doorAnimator == null)
        {
            Debug.LogWarning($"Door animator is not assigned on {name}.", this);
            return;
        }

        if (hasOpened && !allowRepeatedUse)
        {
            return;
        }

        doorAnimator.SetTrigger(openTriggerName);
        hasOpened = true;
    }
}
