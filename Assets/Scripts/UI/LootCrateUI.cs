using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootCrateUI : MonoBehaviour
{
    private static int openLootMenuCount;

    public static bool IsAnyLootMenuOpen => openLootMenuCount > 0;
    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(500f, 360f);
    [SerializeField] private Vector2 slotSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 slotSpacing = new Vector2(8f, 8f);
    [SerializeField] private int slotCount = 15;
    [SerializeField] private int columns = 5;
    [SerializeField] private Vector2 panelOffset = new Vector2(40f, 0f);
    [SerializeField] private bool startHidden = true;

    [Header("Inventory")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private InventoryOverlay playerInventoryOverlay;
    [SerializeField] private Color slotSelectedColor = new Color(1f, 0.4292453f, 0.4292453f, 1f);
    [SerializeField] private Color slotDeselectedColor = new Color(0.6f, 0.6f, 0.6f, 0.35f);

    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas lootCanvas;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private RectTransform slotsRoot;

    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private bool hasInitializedLoot;
    private bool openedPlayerInventoryWithLoot;
    private bool isLootMenuOpen;

    private void Awake()
    {
        if (panelRoot == null)
        {
            BuildLayout();
        }

        if (panelRoot != null && startHidden)
        {
            panelRoot.gameObject.SetActive(false);
        }

        BindInventorySystem();
    }

    public void Toggle(IReadOnlyList<InventoryItemObj> lootItems)
    {
        if (panelRoot == null)
        {
            return;
        }

        bool shouldShow = !panelRoot.gameObject.activeSelf;
        panelRoot.gameObject.SetActive(shouldShow);
        SetLootMenuOpen(shouldShow);

        if (!shouldShow)
        {
            ClosePlayerInventoryCompanion();
            return;
        }

        OpenPlayerInventoryCompanion();

        if (!hasInitializedLoot)
        {
            Populate(lootItems);
            hasInitializedLoot = true;
        }
    }

    private void OpenPlayerInventoryCompanion()
    {
        if (playerInventoryOverlay == null)
        {
            playerInventoryOverlay = FindFirstObjectByType<InventoryOverlay>();
        }

        if (playerInventoryOverlay == null)
        {
            return;
        }

        openedPlayerInventoryWithLoot = !playerInventoryOverlay.IsVisible;
        playerInventoryOverlay.ShowForLootCrate(true);
    }

    private void ClosePlayerInventoryCompanion()
    {
        if (!openedPlayerInventoryWithLoot)
        {
            return;
        }

        if (playerInventoryOverlay == null)
        {
            playerInventoryOverlay = FindFirstObjectByType<InventoryOverlay>();
        }

        if (playerInventoryOverlay != null)
        {
            playerInventoryOverlay.ShowForLootCrate(false);
        }

        openedPlayerInventoryWithLoot = false;
    }

    private void SetLootMenuOpen(bool shouldBeOpen)
    {
        if (shouldBeOpen)
        {
            if (isLootMenuOpen)
            {
                return;
            }

            isLootMenuOpen = true;
            openLootMenuCount++;
            return;
        }

        if (!isLootMenuOpen)
        {
            return;
        }

        isLootMenuOpen = false;
        openLootMenuCount = Mathf.Max(0, openLootMenuCount - 1);
    }

    private void OnDisable()
    {
        SetLootMenuOpen(false);
        ClosePlayerInventoryCompanion();
    }

    private void BuildLayout()
    {
        lootCanvas = CreateCanvas("LootCrateCanvas");
        panelRoot = CreatePanel(lootCanvas.transform, "LootCratePanel");

        var layout = panelRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 12f;
        layout.padding = new RectOffset(16, 16, 16, 16);

        var titleObject = new GameObject("Title", typeof(RectTransform));
        titleObject.layer = LayerMask.NameToLayer("UI");
        titleObject.transform.SetParent(panelRoot, false);
        createdObjects.Add(titleObject);

        var titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = "Loot Crate";
        titleText.fontSize = 28f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        slotsRoot = CreateSlotsRoot(panelRoot, "LootSlotsRoot");
        UpdateSlotsRootSize();
    }

    private Canvas CreateCanvas(string name)
    {
        var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        createdObjects.Add(canvasObject);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

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
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.layer = LayerMask.NameToLayer("UI");
        panelObject.transform.SetParent(parent, false);
        createdObjects.Add(panelObject);

        var rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.sizeDelta = panelSize;
        rectTransform.anchoredPosition = panelOffset;

        var image = panelObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        return rectTransform;
    }

    private RectTransform CreateSlotsRoot(Transform parent, string name)
    {
        var rootObject = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        rootObject.layer = LayerMask.NameToLayer("UI");
        rootObject.transform.SetParent(parent, false);
        createdObjects.Add(rootObject);

        var rectTransform = rootObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        var grid = rootObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = slotSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);
        grid.childAlignment = TextAnchor.UpperCenter;

        for (int i = 0; i < slotCount; i++)
        {
            CreateSlot(rectTransform, i);
        }

        return rectTransform;
    }

    private void CreateSlot(Transform parent, int index)
    {
        var slotObject = new GameObject($"Slot_{index + 1}", typeof(RectTransform), typeof(Image));
        slotObject.layer = LayerMask.NameToLayer("UI");
        slotObject.transform.SetParent(parent, false);
        createdObjects.Add(slotObject);

        var image = slotObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.15f);

        var slotMarker = slotObject.AddComponent<InventorySlotMarker>();
        slotMarker.Setup("RegularSlots", index);

        var slot = slotObject.AddComponent<InventorySlot>();
        slot.Configure(image, slotSelectedColor, slotDeselectedColor);
    }

    private void Populate(IReadOnlyList<InventoryItemObj> lootItems)
    {
        if (slotsRoot == null)
        {
            return;
        }

        UpdateSlotsRootSize();

        var slots = slotsRoot.GetComponentsInChildren<InventorySlot>(true);
        foreach (var slot in slots)
        {
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.transform.GetChild(i).gameObject);
            }
        }

        if (inventorySystem == null)
        {
            return;
        }

        inventorySystem.SetSlots(slots);

        if (lootItems == null)
        {
            return;
        }

        foreach (var item in lootItems)
        {
            inventorySystem.AddItem(item);
        }
    }

    private void BindInventorySystem()
    {
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }
    }

    private void UpdateSlotsRootSize()
    {
        if (slotsRoot == null)
        {
            return;
        }

        int safeColumns = Mathf.Max(1, columns);
        int safeSlots = Mathf.Max(0, slotCount);
        int rows = safeSlots == 0 ? 0 : Mathf.CeilToInt(safeSlots / (float)safeColumns);

        float width = safeColumns * slotSize.x + Mathf.Max(0, safeColumns - 1) * slotSpacing.x;
        float height = rows * slotSize.y + Mathf.Max(0, rows - 1) * slotSpacing.y;

        slotsRoot.sizeDelta = new Vector2(width, height);

        var layoutElement = slotsRoot.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = height;
        }
    }
}
