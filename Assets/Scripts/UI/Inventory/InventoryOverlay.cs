using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryOverlay : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Layout")]
    [SerializeField] private int regularSlotCount = 15;
    [SerializeField] private int chemicalSlotCount = 4;
    [SerializeField] private int weaponSlotCount = 2;
    [SerializeField] private Vector2 slotSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 slotSpacing = new Vector2(8f, 8f);
    [SerializeField] private bool startHidden = true;

    [Header("Cursor")]
    [SerializeField] private bool manageCursor = true;

    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private RectTransform regularSlotsRoot;
    [SerializeField] private RectTransform chemicalSlotsRoot;
    [SerializeField] private RectTransform weaponSlotsRoot;

    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private CursorLockMode previousLockState = CursorLockMode.Locked;
    private bool previousCursorVisible;
    private bool hasSavedCursorState;

    private void Awake()
    {
        if (overlayRoot == null)
        {
            BuildDefaultLayout();
        }

        if (overlayRoot != null)
        {
            if (startHidden)
            {
                overlayRoot.gameObject.SetActive(false);
            }
            else
            {
                SetOverlayVisible(true);
            }
        }
    }

    private void Update()
    {
        if (WasTogglePressed())
        {
            Toggle();
        }
    }

    private bool WasTogglePressed()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return toggleKey switch
        {
            KeyCode.Tab => keyboard.tabKey.wasPressedThisFrame,
            KeyCode.Escape => keyboard.escapeKey.wasPressedThisFrame,
            _ => false
        };
    }

    public void Toggle()
    {
        if (overlayRoot == null)
        {
            return;
        }

        SetOverlayVisible(!overlayRoot.gameObject.activeSelf);
    }

    private void SetOverlayVisible(bool isVisible)
    {
        overlayRoot.gameObject.SetActive(isVisible);

        if (!manageCursor)
        {
            return;
        }

        if (isVisible)
        {
            if (!hasSavedCursorState)
            {
                previousLockState = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                hasSavedCursorState = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (hasSavedCursorState)
        {
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
            hasSavedCursorState = false;
        }
    }

    private void BuildDefaultLayout()
    {
        inventoryCanvas = CreateCanvas("InventoryOverlayCanvas");
        overlayRoot = CreatePanel(inventoryCanvas.transform, "InventoryOverlay");

        var layoutGroup = overlayRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 12f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.padding = new RectOffset(24, 24, 24, 24);

        var fitter = overlayRoot.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        regularSlotsRoot = CreateSlotGroup("RegularSlots", overlayRoot, regularSlotCount, 5);
        chemicalSlotsRoot = CreateSlotGroup("ChemicalSlots", overlayRoot, chemicalSlotCount, 4);
        weaponSlotsRoot = CreateSlotGroup("WeaponSlots", overlayRoot, weaponSlotCount, 2);
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
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(900f, 600f);

        var image = panelObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);

        return rectTransform;
    }

    private RectTransform CreateSlotGroup(string name, RectTransform parent, int slotCount, int columns)
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
        grid.constraintCount = Mathf.Max(1, columns);
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < slotCount; i++)
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
    }
}
