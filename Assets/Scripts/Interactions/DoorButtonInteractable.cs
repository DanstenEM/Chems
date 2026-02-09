using System;
using System.Collections;
using System.Globalization;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorButtonInteractable : MonoBehaviour, IInteractable
{
    private const string DefaultHintFormat = "PRESS {0} OPEN";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    public event Action DoorOpened;

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string openStateName = "ExtractDoorOpen";
    [SerializeField] private bool lockAfterOpen = true;
    [SerializeField] private UnityEvent onDoorOpened;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;

    private bool hasOpened;
    private Coroutine stopRoutine;
    private Collider cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        EnsureHint();
    }

    private void LateUpdate()
    {
        if (hintText == null || !hintText.gameObject.activeSelf)
        {
            return;
        }

        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            return;
        }

        var direction = hintText.transform.position - cameraTarget.transform.position;
        hintText.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Interact()
    {
        if (lockAfterOpen && hasOpened)
        {
            return;
        }

        if (doorAnimator == null)
        {
            Debug.LogWarning("Door animator is not assigned.", this);
            return;
        }

        hasOpened = true;
        DisableInteraction();

        if (stopRoutine != null)
        {
            StopCoroutine(stopRoutine);
        }

        if (!string.IsNullOrWhiteSpace(openTriggerName))
        {
            doorAnimator.SetTrigger(openTriggerName);
        }
        else
        {
            doorAnimator.Play(openStateName, 0, 0f);
        }

        DoorOpened?.Invoke();
        onDoorOpened?.Invoke();

        stopRoutine = StartCoroutine(StopAnimatorAfterPlay());
    }

    public void Active(InputBinding input)
    {
        if (hintText == null)
        {
            return;
        }

        var keyLabel = InputControlPath.ToHumanReadableString(
            input.path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        if (string.IsNullOrWhiteSpace(keyLabel))
        {
            keyLabel = input.path;
        }

        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, keyLabel.ToUpperInvariant());
        hintText.gameObject.SetActive(true);
    }

    public void Deactive()
    {
        if (hintText == null)
        {
            return;
        }

        hintText.gameObject.SetActive(false);
    }

    private void EnsureHint()
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
            return;
        }

        var hintObject = new GameObject("InteractHint");
        hintObject.transform.SetParent(transform, false);
        hintObject.transform.localPosition = hintOffset;

        hintText = hintObject.AddComponent<TextMeshPro>();
        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, "F");
        hintText.fontSize = 3f;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = Color.white;
        hintText.gameObject.SetActive(false);
    }

    private void DisableInteraction()
    {
        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        Deactive();
    }

    private IEnumerator StopAnimatorAfterPlay()
    {
        var clipLength = GetAnimationClipLength(openStateName);
        if (clipLength > 0f)
        {
            yield return new WaitForSeconds(clipLength);
            doorAnimator.enabled = false;
        }
    }

    private float GetAnimationClipLength(string clipName)
    {
        if (doorAnimator == null || doorAnimator.runtimeAnimatorController == null)
        {
            return 0f;
        }

        foreach (var clip in doorAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip.length;
            }
        }

        return 0f;
    }
}
