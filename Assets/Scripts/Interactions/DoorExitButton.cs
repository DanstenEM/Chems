using System.Globalization;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorExitButton : MonoBehaviour, IInteractable
{
    private static DoorExitButton activeHintOwner;
    private const string DefaultHintFormat = "PRESS {0} INTERACT";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private bool allowRepeatedUse;

    private bool hasOpened;

    private void Awake()
    {
        EnsureHint();

        if (doorAnimator == null)
        {
            doorAnimator = GetComponentInParent<Animator>();
        }
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
}
