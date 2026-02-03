using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerOverlay : MonoBehaviour
{
    private static bool hasAutoSpawned;

    [Header("Health")]
    [SerializeField] private Health health;
    [SerializeField] private string healthLabel = "Health";
    [SerializeField] private TMP_FontAsset fallbackFont;

    [Header("Layout")]
    [SerializeField] private Vector2 panelPadding = new Vector2(18f, 12f);
    [SerializeField] private Vector2 panelOffset = new Vector2(24f, 24f);
    [SerializeField] private Vector2 panelSize = new Vector2(260f, 64f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 28;

    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private RectTransform weaponSlotsRoot;

    [Header("Weapon Slots")]
    [SerializeField] private int weaponSlotCount = 2;
    [SerializeField] private Vector2 weaponSlotSize = new Vector2(56f, 56f);
    [SerializeField] private Vector2 weaponSlotOffset = new Vector2(-24f, 24f);
    [SerializeField] private float weaponSlotSpacing = 8f;
    [SerializeField] private Color weaponSlotBackground = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color weaponSlotOccupiedBackground = new Color(0.95f, 0.2f, 0.2f, 0.35f);
    [SerializeField] private Color weaponSlotCountColor = Color.white;
    [SerializeField] private InventorySystem inventorySystem;

    private Image[] weaponSlotImages;
    private Image[] weaponIconImages;
    private TextMeshProUGUI[] weaponCountTexts;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureOverlayExists()
    {
        if (hasAutoSpawned || FindObjectOfType<PlayerOverlay>() != null)
        {
            return;
        }

        var overlayObject = new GameObject("PlayerOverlay");
        DontDestroyOnLoad(overlayObject);
        overlayObject.AddComponent<PlayerOverlay>();
        hasAutoSpawned = true;
    }

    private void Awake()
    {
        if (overlayRoot == null)
        {
            BuildDefaultLayout();
        }

        if (health == null)
        {
            health = FindPlayerHealth();
        }

        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        UpdateHealthText();
        UpdateWeaponSlots();
    }

    private void Update()
    {
        UpdateHealthText();
        UpdateWeaponSlots();
        HandleWeaponSlotInput();
    }

    private void UpdateHealthText()
    {
        if (healthText == null)
        {
            return;
        }

        if (health == null)
        {
            healthText.text = $"{healthLabel}: --";
            return;
        }

        healthText.text = $"{healthLabel}: {health.CurrentHealth:0}";
    }

    private void UpdateWeaponSlots()
    {
        if (weaponIconImages == null || weaponIconImages.Length == 0)
        {
            return;
        }

        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        var slots = GetInventorySlots();
        var weaponSlots = GetWeaponSlots(slots);

        for (int i = 0; i < weaponIconImages.Length; i++)
        {
            var iconImage = weaponIconImages[i];
            var slot = i < weaponSlots.Length ? weaponSlots[i] : null;
            var item = slot != null ? slot.GetComponentInChildren<InventoryItem>() : null;
            var countText = weaponCountTexts != null && i < weaponCountTexts.Length ? weaponCountTexts[i] : null;

            if (item != null && item.itemObj != null && item.itemObj.icon != null)
            {
                if (weaponSlotImages != null && i < weaponSlotImages.Length)
                {
                    weaponSlotImages[i].color = GetWeaponSlotColor(item.itemObj.category);
                }

                iconImage.enabled = true;
                iconImage.sprite = item.itemObj.icon;
                iconImage.color = Color.white;

                if (countText != null)
                {
                    countText.enabled = true;
                    countText.text = item.count.ToString();
                }
            }
            else if (item != null)
            {
                if (weaponSlotImages != null && i < weaponSlotImages.Length)
                {
                    weaponSlotImages[i].color = GetWeaponSlotColor(item.itemObj != null ? item.itemObj.category : InventoryItemObj.ItemCategory.Weapon);
                }

                iconImage.enabled = false;
                iconImage.sprite = null;

                if (countText != null)
                {
                    countText.enabled = true;
                    countText.text = item.count.ToString();
                }
            }
            else
            {
                if (weaponSlotImages != null && i < weaponSlotImages.Length)
                {
                    weaponSlotImages[i].color = weaponSlotBackground;
                }

                iconImage.enabled = false;
                iconImage.sprite = null;

                if (countText != null)
                {
                    countText.enabled = false;
                    countText.text = string.Empty;
                }
            }
        }
    }

    private InventorySlot[] GetInventorySlots()
    {
        if (inventorySystem != null)
        {
            var slots = inventorySystem.GetSlots();
            if (slots != null && slots.Length > 0)
            {
                return slots;
            }
        }

        var allSlots = Resources.FindObjectsOfTypeAll<InventorySlot>();
        if (allSlots == null || allSlots.Length == 0)
        {
            return new InventorySlot[0];
        }

        var sceneSlots = new System.Collections.Generic.List<InventorySlot>();
        foreach (var slot in allSlots)
        {
            if (slot != null && slot.gameObject.scene.IsValid())
            {
                sceneSlots.Add(slot);
            }
        }

        return sceneSlots.ToArray();
    }

    private InventorySlot[] GetWeaponSlots(InventorySlot[] slots)
    {
        if (slots == null || slots.Length == 0)
        {
            return new InventorySlot[0];
        }

        var weaponSlots = new System.Collections.Generic.List<InventorySlot>();
        foreach (var slot in slots)
        {
            if (slot == null)
            {
                continue;
            }

            var marker = slot.GetComponent<InventorySlotMarker>();
            if (marker != null && marker.Category == InventorySlotMarker.SlotCategory.Weapon)
            {
                weaponSlots.Add(slot);
            }
        }

        weaponSlots.Sort((left, right) =>
        {
            var leftMarker = left.GetComponent<InventorySlotMarker>();
            var rightMarker = right.GetComponent<InventorySlotMarker>();
            var leftIndex = leftMarker != null ? leftMarker.Index : int.MaxValue;
            var rightIndex = rightMarker != null ? rightMarker.Index : int.MaxValue;
            return leftIndex.CompareTo(rightIndex);
        });

        return weaponSlots.ToArray();
    }

    private void HandleWeaponSlotInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        if (inventorySystem == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            inventorySystem.SelectWeaponSlot(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            inventorySystem.SelectWeaponSlot(1);
        }
    }

    private Color GetWeaponSlotColor(InventoryItemObj.ItemCategory category)
    {
        return category switch
        {
            InventoryItemObj.ItemCategory.Chemical => new Color(0.2f, 0.9f, 0.2f, 1f),
            InventoryItemObj.ItemCategory.Weapon => new Color(0.95f, 0.2f, 0.2f, 1f),
            _ => new Color(1f, 0.85f, 0.2f, 1f)
        };
    }

    private Health FindPlayerHealth()
    {
        var healths = FindObjectsOfType<Health>();
        foreach (var candidate in healths)
        {
            if (candidate != null && candidate.CompareTag("Player"))
            {
                return candidate;
            }
        }

        return healths.Length > 0 ? healths[0] : null;
    }

    private void BuildDefaultLayout()
    {
        overlayCanvas = CreateCanvas("PlayerOverlayCanvas");
        overlayRoot = CreatePanel(overlayCanvas.transform, "PlayerOverlayPanel");
        healthText = CreateLabel(overlayRoot, "HealthText");
        weaponSlotsRoot = CreateWeaponSlots(overlayCanvas.transform, "WeaponSlotsOverlay");
    }

    private Canvas CreateCanvas(string name)
    {
        var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var rectTransform = canvasObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return canvas;
    }

    private RectTransform CreatePanel(Transform parent, string name)
    {
        var panelObject = new GameObject(name, typeof(RectTransform));
        panelObject.layer = LayerMask.NameToLayer("UI");
        panelObject.transform.SetParent(parent, false);

        var rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.sizeDelta = panelSize;
        rectTransform.anchoredPosition = panelOffset;

        return rectTransform;
    }

    private TextMeshProUGUI CreateLabel(RectTransform parent, string name)
    {
        var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(parent, false);

        var rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.offsetMin = new Vector2(panelPadding.x, panelPadding.y);
        rectTransform.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);

        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.color = textColor;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
        label.text = $"{healthLabel}: --";
        label.raycastTarget = false;

        if (label.font == null)
        {
            label.font = fallbackFont != null ? fallbackFont : TMP_Settings.defaultFontAsset;
        }

        return label;
    }

    private RectTransform CreateWeaponSlots(Transform parent, string name)
    {
        var rootObject = new GameObject(name, typeof(RectTransform));
        rootObject.layer = LayerMask.NameToLayer("UI");
        rootObject.transform.SetParent(parent, false);

        var rectTransform = rootObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0f);
        rectTransform.sizeDelta = new Vector2(
            weaponSlotSize.x * weaponSlotCount + weaponSlotSpacing * (weaponSlotCount - 1),
            weaponSlotSize.y);
        rectTransform.anchoredPosition = new Vector2(weaponSlotOffset.x, weaponSlotOffset.y);

        weaponSlotImages = new Image[weaponSlotCount];
        weaponIconImages = new Image[weaponSlotCount];
        weaponCountTexts = new TextMeshProUGUI[weaponSlotCount];

        for (int i = 0; i < weaponSlotCount; i++)
        {
            var slot = CreateWeaponSlot(rectTransform, $"WeaponSlot_{i + 1}", i);
            weaponSlotImages[i] = slot.background;
            weaponIconImages[i] = slot.icon;
            weaponCountTexts[i] = slot.count;
        }

        return rectTransform;
    }

    private (Image background, Image icon, TextMeshProUGUI count) CreateWeaponSlot(RectTransform parent, string name, int index)
    {
        var slotObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        slotObject.layer = LayerMask.NameToLayer("UI");
        slotObject.transform.SetParent(parent, false);

        var rectTransform = slotObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0f);
        rectTransform.sizeDelta = weaponSlotSize;
        rectTransform.anchoredPosition = new Vector2(
            -(weaponSlotSize.x + weaponSlotSpacing) * (weaponSlotCount - 1 - index),
            0f);

        var background = slotObject.GetComponent<Image>();
        background.color = weaponSlotBackground;
        background.raycastTarget = false;

        var iconObject = new GameObject($"{name}_Icon", typeof(RectTransform), typeof(Image));
        iconObject.layer = LayerMask.NameToLayer("UI");
        iconObject.transform.SetParent(slotObject.transform, false);

        var iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(6f, 6f);
        iconRect.offsetMax = new Vector2(-6f, -6f);

        var iconImage = iconObject.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.enabled = false;
        iconImage.raycastTarget = false;

        var countObject = new GameObject($"{name}_Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        countObject.layer = LayerMask.NameToLayer("UI");
        countObject.transform.SetParent(slotObject.transform, false);

        var countRect = countObject.GetComponent<RectTransform>();
        countRect.anchorMin = Vector2.zero;
        countRect.anchorMax = Vector2.one;
        countRect.offsetMin = new Vector2(4f, 2f);
        countRect.offsetMax = new Vector2(-4f, -2f);

        var countText = countObject.GetComponent<TextMeshProUGUI>();
        countText.color = weaponSlotCountColor;
        countText.fontSize = 22;
        countText.alignment = TextAlignmentOptions.BottomRight;
        countText.text = string.Empty;
        countText.enabled = false;
        countText.raycastTarget = false;

        if (countText.font == null)
        {
            countText.font = fallbackFont != null ? fallbackFont : TMP_Settings.defaultFontAsset;
        }

        return (background, iconImage, countText);
    }
}
