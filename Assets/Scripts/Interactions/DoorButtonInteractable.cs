using System.Globalization;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorButtonInteractable : MonoBehaviour, IInteractable
{
    private static DoorButtonInteractable activeHintOwner;
    private const string DefaultHintFormat = "PRESS {0} OPEN";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private bool playOnce = true;
    private bool hasPlayed;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;
    [SerializeField, Range(-1f, 1f)] private float lookDotThreshold = 0.75f;
    private bool isActive;

    private void Awake()
    {
        EnsureHint();

        if (doorAnimator == null)
        {
            doorAnimator = GetComponentInParent<Animator>();
        }

        if (doorAnimator == null)
        {
            doorAnimator = transform.root.GetComponentInChildren<Animator>();
        }
    }

    private void LateUpdate()
    {
        if (hintText == null || !isActive)
        {
            return;
        }

        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            return;
        }

        UpdateLookHint(cameraTarget);

        if (!hintText.gameObject.activeSelf)
        {
            return;
        }

        var direction = hintText.transform.position - cameraTarget.transform.position;
        hintText.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Interact()
    {
        if (playOnce && hasPlayed)
        {
            return;
        }

        if (doorAnimator == null)
        {
            return;
        }

        doorAnimator.ResetTrigger(openTriggerName);
        doorAnimator.SetTrigger(openTriggerName);
        hasPlayed = true;
    }

    public void Active(InputBinding input)
    {
        if (hintText == null)
        {
            return;
        }

        if (activeHintOwner != null && activeHintOwner != this)
        {
            activeHintOwner.Deactive();
        }

        activeHintOwner = this;
        isActive = true;

        var keyLabel = InputControlPath.ToHumanReadableString(
            input.path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        if (string.IsNullOrWhiteSpace(keyLabel))
        {
            keyLabel = input.path;
        }

        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, keyLabel.ToUpperInvariant());
        UpdateLookHint(Camera.main);
    }

    public void Deactive()
    {
        if (hintText == null)
        {
            return;
        }

        hintText.gameObject.SetActive(false);
        isActive = false;

        if (activeHintOwner == this)
        {
            activeHintOwner = null;
        }
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
        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, "E");
        hintText.fontSize = 3.5f;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = Color.white;
        hintText.gameObject.SetActive(false);
    }

    private void UpdateLookHint(Camera cameraTarget)
    {
        if (cameraTarget == null || hintText == null)
        {
            return;
        }

        var direction = (transform.position - cameraTarget.transform.position).normalized;
        float dot = Vector3.Dot(cameraTarget.transform.forward, direction);
        hintText.gameObject.SetActive(dot >= lookDotThreshold);
    }
}
