using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LootCrateOverlay : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Layout")]
    [SerializeField] private int slotCount = 15;
    [SerializeField] private int columns = 5;
    [SerializeField] private Vector2 slotSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 slotSpacing = new Vector2(8f, 8f);
    [SerializeField] private Vector2 panelSize = new Vector2(900f, 600f);
    [SerializeField] private Vector2 leftSideOffset = new Vector2(40f, 0f);
    [SerializeField] private bool startHidden = true;

    [Header("Inventory")]
    [SerializeField] private InventoryItem inventoryItemPrefab;
    [SerializeField] private InputActionProperty mouseAction;

    [Header("Scene References (Optional)")]
    [SerializeField] private InventoryOverlay playerInventoryOverlay;
    [SerializeField] private Canvas lootCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private RectTransform lootSlotsRoot;
    [SerializeField] private InventorySystem inventorySystem;

    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private LootCrate activeCrate;
    private bool previousPlayerVisible;
    private InventoryOverlay.OverlaySide previousPlayerSide;

    private void Awake()
    {
        if (overlayRoot == null)
        {
            BuildDefaultLayout();
        }

        if (playerInventoryOverlay == null)
        {
            playerInventoryOverlay = FindObjectOfType<InventoryOverlay>();
        }

        BindInventorySystem();

        if (overlayRoot != null && startHidden)
        {
            overlayRoot.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (activeCrate != null && Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    public void Open(LootCrate lootCrate)
    {
        if (lootCrate == null || overlayRoot == null)
        {
            return;
        }

        if (activeCrate != null && activeCrate != lootCrate)
        {
            Close();
        }

        activeCrate = lootCrate;
        activeCrate.GenerateLootIfNeeded();
        PopulateFromCrate(activeCrate);

        overlayRoot.gameObject.SetActive(true);
        ShiftPlayerOverlay(true);
    }

    public void Close()
    {
        if (activeCrate != null)
        {
            activeCrate.SyncLootFromSlots(inventorySystem != null ? inventorySystem.Slots : null);
        }

        activeCrate = null;

        if (overlayRoot != null)
        {
            overlayRoot.gameObject.SetActive(false);
        }

        ShiftPlayerOverlay(false);
    }

    private void ShiftPlayerOverlay(bool isOpening)
    {
        if (playerInventoryOverlay == null)
        {
            return;
        }

        if (isOpening)
        {
            previousPlayerVisible = playerInventoryOverlay.IsVisible;
            previousPlayerSide = playerInventoryOverlay.CurrentSide;
            playerInventoryOverlay.SetVisible(true);
            playerInventoryOverlay.SetOverlaySide(InventoryOverlay.OverlaySide.Right);
        }
        else
        {
            playerInventoryOverlay.SetOverlaySide(previousPlayerSide);
            playerInventoryOverlay.SetVisible(previousPlayerVisible);
        }
    }

    private void PopulateFromCrate(LootCrate lootCrate)
    {
        if (inventorySystem == null)
        {
            return;
        }

        ClearSlots();

        foreach (var item in lootCrate.GetLootItems())
        {
            if (item == null)
            {
                continue;
            }

            inventorySystem.AddItem(item);
        }
    }

    private void ClearSlots()
    {
        if (inventorySystem == null || inventorySystem.Slots == null)
        {
            return;
        }

        foreach (var slot in inventorySystem.Slots)
        {
            if (slot == null)
            {
                continue;
            }

            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.transform.GetChild(i).gameObject);
            }
        }
    }

    private void BuildDefaultLayout()
    {
        lootCanvas = CreateCanvas("LootCrateOverlayCanvas");
        overlayRoot = CreatePanel(lootCanvas.transform, "LootCrateOverlay");
        lootSlotsRoot = CreateSlotGroup("LootSlots", overlayRoot, slotCount, columns);
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
        rectTransform.anchoredPosition = leftSideOffset;

        var image = panelObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        return rectTransform;
    }

    private RectTransform CreateSlotGroup(string name, RectTransform parent, int slots, int columnCount)
    {
        var groupObject = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup));
        groupObject.layer = LayerMask.NameToLayer("UI");
        groupObject.transform.SetParent(parent, false);
        createdObjects.Add(groupObject);

        var groupRect = groupObject.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.5f);
        groupRect.anchorMax = new Vector2(0.5f, 0.5f);
        groupRect.pivot = new Vector2(0.5f, 0.5f);

        var grid = groupObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = slotSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columnCount);
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < slots; i++)
        {
            CreateSlot(groupRect, name, i);
        }

        return groupRect;
    }

    private void CreateSlot(Transform parent, string groupName, int index)
    {
        var slotObject = new GameObject($"{groupName}_Slot_{index + 1}", typeof(RectTransform), typeof(Image));
        slotObject.layer = LayerMask.NameToLayer("UI");
        slotObject.transform.SetParent(parent, false);
        createdObjects.Add(slotObject);

        var image = slotObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.15f);

        var slotMarker = slotObject.AddComponent<InventorySlotMarker>();
        slotMarker.Setup(groupName, index);

        var slot = slotObject.AddComponent<InventorySlot>();
        slot.Configure(image, new Color(1f, 0.4292453f, 0.4292453f, 1f), new Color(0.6f, 0.6f, 0.6f, 0.35f));
    }

    private void BindInventorySystem()
    {
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }

        if (inventorySystem == null)
        {
            inventorySystem = gameObject.AddComponent<InventorySystem>();
        }

        if (playerInventoryOverlay != null && inventoryItemPrefab == null)
        {
            inventoryItemPrefab = playerInventoryOverlay.InventorySystem != null
                ? playerInventoryOverlay.InventorySystem.InventoryPrefab
                : inventoryItemPrefab;
        }

        if (playerInventoryOverlay != null && mouseAction.action == null && playerInventoryOverlay.InventorySystem != null)
        {
            mouseAction = playerInventoryOverlay.InventorySystem.mouseAction;
        }

        if (overlayRoot != null)
        {
            var slots = overlayRoot.GetComponentsInChildren<InventorySlot>(true);
            inventorySystem.SetSlots(slots);
        }

        inventorySystem.Configure(inventoryItemPrefab, mouseAction);
    }
}
