using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class InventoryLayout : MonoBehaviour
{
    [Header("Slot Prefab")]
    [SerializeField] private InventorySlot slotPrefab;

    [Header("Counts")]
    [SerializeField] private int weaponSlots = 2;
    [SerializeField] private int armorSlots = 1;
    [SerializeField] private int supportSlots = 1;
    [SerializeField] private int lootSlots = 15;
    [SerializeField] private int chemicalSlots = 4;

    [Header("Layout")]
    [SerializeField] private Vector2 equipmentCellSize = new(120f, 120f);
    [SerializeField] private Vector2 lootCellSize = new(110f, 110f);
    [SerializeField] private Vector2 chemicalCellSize = new(100f, 100f);
    [SerializeField] private Vector2 cellSpacing = new(10f, 10f);
    [SerializeField] private int lootColumns = 5;

    [Header("Styling")]
    [SerializeField] private Color sectionBackground = new(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private int labelFontSize = 24;
    [SerializeField] private TMP_FontAsset labelFont;

    [Header("Toggle")]
    [SerializeField] private bool startHidden = true;

    private readonly List<InventorySlot> slots = new();
    private CanvasGroup canvasGroup;
    private InputAction toggleAction;
    private RectTransform contentRoot;
    private bool isVisible = true;

    public InventorySlot[] Slots => slots.ToArray();

    private void Awake()
    {
        contentRoot = GetComponent<RectTransform>();
        if (labelFont == null)
        {
            labelFont = TMP_Settings.defaultFontAsset;
        }

        canvasGroup = GetOrAdd<CanvasGroup>();
        toggleAction = new InputAction("ToggleInventory", InputActionType.Button, "<Keyboard>/tab");
        ConfigureRootLayout();

        if (startHidden)
        {
            SetVisible(false);
        }
    }

    private void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += HandleToggle;
    }

    private void OnDisable()
    {
        toggleAction.performed -= HandleToggle;
        toggleAction.Disable();
    }

    public InventorySlot[] BuildLayout()
    {
        if (slotPrefab == null)
        {
            Debug.LogWarning("InventoryLayout requires a slot prefab reference.");
            return Slots;
        }

        ClearChildren();
        slots.Clear();

        var equipmentSlotsCount = weaponSlots + armorSlots + supportSlots;
        CreateSection(
            "EQUIPMENT",
            equipmentSlotsCount,
            4,
            equipmentCellSize,
            CreateEquipmentSlotLabels);

        CreateSection(
            "LOOT",
            lootSlots,
            lootColumns,
            lootCellSize,
            null);

        CreateSection(
            "CHEMICALS",
            chemicalSlots,
            chemicalSlots,
            chemicalCellSize,
            null);

        return Slots;
    }

    private void HandleToggle(InputAction.CallbackContext context)
    {
        SetVisible(!isVisible);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }

    private void ConfigureRootLayout()
    {
        var gridLayout = GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.enabled = false;
        }

        var verticalLayout = GetOrAdd<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.spacing = 18f;
        verticalLayout.padding = new RectOffset(24, 24, 24, 24);

        var sizeFitter = GetOrAdd<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void CreateSection(
        string title,
        int slotCount,
        int columnCount,
        Vector2 cellSize,
        System.Action<InventorySlot[], RectTransform> configureSlots)
    {
        var section = new GameObject($"{title} Section", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        section.transform.SetParent(contentRoot, false);

        var sectionImage = section.GetComponent<Image>();
        sectionImage.color = sectionBackground;

        var sectionLayout = section.GetComponent<VerticalLayoutGroup>();
        sectionLayout.childAlignment = TextAnchor.UpperLeft;
        sectionLayout.spacing = 12f;
        sectionLayout.padding = new RectOffset(16, 16, 12, 16);

        CreateLabel(title, section.transform as RectTransform);

        var gridObject = new GameObject($"{title} Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        gridObject.transform.SetParent(section.transform, false);

        var gridLayout = gridObject.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = cellSize;
        gridLayout.spacing = cellSpacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columnCount;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        var gridFitter = gridObject.GetComponent<ContentSizeFitter>();
        gridFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var createdSlots = new InventorySlot[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            var slotInstance = Instantiate(slotPrefab, gridObject.transform);
            slotInstance.name = $"{title} Slot {i + 1:00}";
            slots.Add(slotInstance);
            createdSlots[i] = slotInstance;
        }

        configureSlots?.Invoke(createdSlots, gridObject.transform as RectTransform);
    }

    private void CreateEquipmentSlotLabels(InventorySlot[] equipmentSlots, RectTransform grid)
    {
        var labels = new List<string>();
        for (var i = 1; i <= weaponSlots; i++)
        {
            labels.Add($"WEAPON {i}");
        }

        for (var i = 0; i < armorSlots; i++)
        {
            labels.Add("ARMOR");
        }

        for (var i = 0; i < supportSlots; i++)
        {
            labels.Add("SUPPORT");
        }

        for (var i = 0; i < equipmentSlots.Length && i < labels.Count; i++)
        {
            var slotLabel = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            slotLabel.transform.SetParent(equipmentSlots[i].transform, false);

            var labelRect = slotLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(0f, 26f);
            labelRect.anchoredPosition = new Vector2(0f, 4f);

            var labelText = slotLabel.GetComponent<TextMeshProUGUI>();
            labelText.text = labels[i];
            labelText.color = labelColor;
            labelText.fontSize = labelFontSize - 6;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.font = labelFont;
            labelText.enableAutoSizing = false;
        }
    }

    private void CreateLabel(string text, RectTransform parent)
    {
        var labelObject = new GameObject($"{text} Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.sizeDelta = new Vector2(0f, 32f);

        var labelText = labelObject.GetComponent<TextMeshProUGUI>();
        labelText.text = text;
        labelText.color = labelColor;
        labelText.fontSize = labelFontSize;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.font = labelFont;
    }

    private void ClearChildren()
    {
        for (var i = contentRoot.childCount - 1; i >= 0; i--)
        {
            var child = contentRoot.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private T GetOrAdd<T>() where T : Component
    {
        if (TryGetComponent(out T component))
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }
}
