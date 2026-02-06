using System.Collections;
using Assets.Scripts.Interactions.Abstract;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ExitDoorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyActiveType _keyType = KeyActiveType.Tap;
    public KeyActiveType keyType { get => _keyType; set => _keyType = value; }

    [Header("Door Animation")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string openStateName = "";

    [Header("Fallback (if Animator has no open trigger/state)")]
    [SerializeField] private Transform doorToMove;
    [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 4f, 0f);
    [SerializeField, Min(0.01f)] private float fallbackOpenDuration = 1.25f;

    [Header("Behavior")]
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private UnityEvent onDoorOpened;

    private bool isOpened;
    private Coroutine openingRoutine;

    public void Interact()
    {
        if (oneTimeUse && isOpened)
        {
            return;
        }

        if (TryTriggerAnimatorOpen())
        {
            isOpened = true;
            onDoorOpened?.Invoke();
            return;
        }

        if (doorToMove == null)
        {
            Debug.LogWarning($"{nameof(ExitDoorButton)} on '{name}' has no valid door target to open.", this);
            return;
        }

        if (openingRoutine != null)
        {
            StopCoroutine(openingRoutine);
        }

        openingRoutine = StartCoroutine(OpenDoorRoutine());
    }

    public void Active(InputBinding input)
    {
    }

    public void Deactive()
    {
    }

    private bool TryTriggerAnimatorOpen()
    {
        if (doorAnimator == null || doorAnimator.runtimeAnimatorController == null)
        {
            return false;
        }

        bool triggered = false;

        if (!string.IsNullOrWhiteSpace(openTriggerName))
        {
            int triggerHash = Animator.StringToHash(openTriggerName);
            doorAnimator.ResetTrigger(triggerHash);
            doorAnimator.SetTrigger(triggerHash);
            triggered = true;
        }

        if (!string.IsNullOrWhiteSpace(openStateName))
        {
            doorAnimator.Play(openStateName, 0, 0f);
            triggered = true;
        }

        return triggered;
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpened = true;

        Vector3 startPosition = doorToMove.localPosition;
        Vector3 endPosition = startPosition + openLocalOffset;
        float elapsed = 0f;

        while (elapsed < fallbackOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallbackOpenDuration);
            doorToMove.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        doorToMove.localPosition = endPosition;
        openingRoutine = null;
        onDoorOpened?.Invoke();
    }
}
