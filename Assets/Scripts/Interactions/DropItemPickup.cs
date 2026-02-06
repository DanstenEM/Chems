using System.Globalization;
using Assets.Scripts.Interactions.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DropItemPickup : MonoBehaviour, IInteractable
{
    private static DropItemPickup activeHintOwner;
    private const string DefaultHintFormat = "PRESS {0} TAKE";

    [field: SerializeField] public KeyActiveType keyType { get; set; } = KeyActiveType.Tap;

    [Header("Item")]
    [SerializeField] private InventoryItemObj itemObj;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 hintOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string hintFormat = DefaultHintFormat;

    [Header("Look Hint")]
    [SerializeField] private bool showHintOnLook = true;
    [SerializeField] private float lookRayDistance = 4f;
    [SerializeField] private LayerMask lookLayers = ~0;

    [Header("Drop Physics")]
    [SerializeField] private bool useDropPhysics = true;
    [SerializeField] private float gravityAcceleration = 20f;
    [SerializeField] private float maxFallSpeed = 25f;
    [SerializeField] private float groundRaycastHeight = 1.5f;
    [SerializeField] private float groundRaycastDistance = 6f;
    [SerializeField] private float groundOffset = 0.02f;
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Pickup Area")]
    [SerializeField] private float pickupAreaMultiplier = 3f;

    private InventorySystem inventorySystem;
    private Collider pickupCollider;
    private bool isPickedUp;
    private bool isTriggerHintActive;
    private bool isGrounded;
    private float currentFallSpeed;
    private string currentHintLabel = "E";

    private void Awake()
    {
        inventorySystem = InventorySystem.GameplayInventory;
        pickupCollider = GetComponent<Collider>();
        EnsureHint();
        InitializeDropState();
        ExpandPickupArea();
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

    private void Update()
    {
        SimulateDropPhysics();
        UpdateLookHint();
    }

    public void Interact()
    {
        if (isPickedUp)
        {
            return;
        }

        if (itemObj == null)
        {
            return;
        }

        if (inventorySystem == null)
        {
            inventorySystem = InventorySystem.GameplayInventory;
        }

        if (inventorySystem != null && inventorySystem.AddItem(itemObj))
        {
            isPickedUp = true;
            if (hintText != null)
            {
                hintText.gameObject.SetActive(false);
            }

            if (TryGetComponent<Collider>(out var pickupCollider))
            {
                pickupCollider.enabled = false;
            }

            Destroy(gameObject);
        }
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
        isTriggerHintActive = true;

        UpdateHintLabel(input);
        ShowHint();
    }

    public void Deactive()
    {
        if (hintText == null)
        {
            return;
        }

        isTriggerHintActive = false;

        if (!showHintOnLook || !IsLookedAt())
        {
            HideHint();
        }

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

        var hintObject = new GameObject("PickupHint");
        hintObject.transform.SetParent(transform, false);
        hintObject.transform.localPosition = hintOffset;

        hintText = hintObject.AddComponent<TextMeshPro>();
        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, "E");
        hintText.fontSize = 3.5f;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = Color.white;
        hintText.gameObject.SetActive(false);
    }

    private void UpdateHintLabel(InputBinding input)
    {
        var keyLabel = InputControlPath.ToHumanReadableString(
            input.path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        if (string.IsNullOrWhiteSpace(keyLabel))
        {
            keyLabel = input.path;
        }

        currentHintLabel = keyLabel.ToUpperInvariant();
        if (hintText != null)
        {
            hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, currentHintLabel);
        }
    }

    private void UpdateLookHint()
    {
        if (!showHintOnLook || hintText == null || isPickedUp)
        {
            return;
        }

        if (IsLookedAt())
        {
            ShowHint();
            return;
        }

        if (!isTriggerHintActive)
        {
            HideHint();
        }
    }

    private bool IsLookedAt()
    {
        if (pickupCollider == null)
        {
            return false;
        }

        var cameraTarget = Camera.main;
        if (cameraTarget == null)
        {
            return false;
        }

        var ray = new Ray(cameraTarget.transform.position, cameraTarget.transform.forward);
        if (Physics.Raycast(ray, out var hit, lookRayDistance, lookLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider == pickupCollider)
            {
                return true;
            }

            return hit.collider.transform.IsChildOf(transform);
        }

        return false;
    }

    private void ShowHint()
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
        hintText.text = string.Format(CultureInfo.InvariantCulture, hintFormat, currentHintLabel);
        hintText.gameObject.SetActive(true);
    }

    private void HideHint()
    {
        if (hintText == null)
        {
            return;
        }

        hintText.gameObject.SetActive(false);
    }

    private void InitializeDropState()
    {
        if (!useDropPhysics)
        {
            SnapToGround();
            isGrounded = true;
            return;
        }

        isGrounded = false;
        currentFallSpeed = 0f;
    }

    private void SimulateDropPhysics()
    {
        if (!useDropPhysics || isGrounded || isPickedUp)
        {
            return;
        }

        var startPosition = transform.position + Vector3.up * groundRaycastHeight;
        var rayDistance = groundRaycastHeight + groundRaycastDistance;

        if (!Physics.Raycast(startPosition, Vector3.down, out var hit, rayDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            currentFallSpeed = Mathf.Min(currentFallSpeed + gravityAcceleration * Time.deltaTime, maxFallSpeed);
            transform.position += Vector3.down * (currentFallSpeed * Time.deltaTime);
            return;
        }

        float targetY = hit.point.y + groundOffset;
        currentFallSpeed = Mathf.Min(currentFallSpeed + gravityAcceleration * Time.deltaTime, maxFallSpeed);
        float nextY = transform.position.y - (currentFallSpeed * Time.deltaTime);

        if (nextY <= targetY)
        {
            var snappedPosition = transform.position;
            snappedPosition.y = targetY;
            transform.position = snappedPosition;
            currentFallSpeed = 0f;
            isGrounded = true;
            return;
        }

        var nextPosition = transform.position;
        nextPosition.y = nextY;
        transform.position = nextPosition;
    }

    private void SnapToGround()
    {
        var startPosition = transform.position + Vector3.up * groundRaycastHeight;
        var rayDistance = groundRaycastHeight + groundRaycastDistance;
        if (Physics.Raycast(startPosition, Vector3.down, out var hit, rayDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            var alignedPosition = hit.point;
            alignedPosition.y += groundOffset;
            transform.position = alignedPosition;
        }
    }

    private void ExpandPickupArea()
    {
        if (!TryGetComponent<Collider>(out var targetCollider))
        {
            return;
        }

        float multiplier = Mathf.Max(1f, pickupAreaMultiplier);

        if (targetCollider is SphereCollider sphere)
        {
            sphere.radius *= multiplier;
            return;
        }

        if (targetCollider is CapsuleCollider capsule)
        {
            capsule.radius *= multiplier;
            capsule.height *= multiplier;
            return;
        }

        if (targetCollider is BoxCollider box)
        {
            box.size = box.size * multiplier;
        }
    }
}
