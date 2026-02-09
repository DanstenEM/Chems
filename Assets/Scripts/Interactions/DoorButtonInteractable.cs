using System.Globalization;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorButtonInteractable : MonoBehaviour, IInteractable
{
    private const string DefaultHintFormat = "PRESS {0} OPEN";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openStateName = "ExtractDoorOpen";
    [SerializeField] private bool lockAfterOpen = true;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;

    private bool hasOpened;

    private void Awake()
    {
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

        doorAnimator.Play(openStateName, 0, 0f);
        hasOpened = true;
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
}
