using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponQuickbar : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureQuickbarExists()
    {
        if (FindObjectOfType<WeaponQuickbar>() != null)
        {
            return;
        }

        var quickbarObject = new GameObject("WeaponQuickbar");
        quickbarObject.AddComponent<WeaponQuickbar>();
    }

    [Header("Layout")]
    [SerializeField] private int weaponSlotCount = 2;
    [SerializeField] private Vector2 slotSize = new Vector2(300f, 150f);
    [SerializeField] private Vector2 slotSpacing = new Vector2(0f, 16f);
    [SerializeField] private Vector2 anchoredPosition = new Vector2(-32f, 32f);

    [Header("Appearance")]
    [SerializeField] private Color slotSelectedColor = new Color(1f, 0.4292453f, 0.4292453f, 1f);
    [SerializeField] private Color slotDeselectedColor = new Color(0.6f, 0.6f, 0.6f, 0.35f);
    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas quickbarCanvas;
    [SerializeField] private RectTransform slotsRoot;
    [SerializeField] private InventorySystem inventorySystem;

    private readonly List<InventorySlot> weaponSlots = new List<InventorySlot>();
    private int selectedIndex;

    public bool HasSelectedWeapon => GetSelectedWeaponItem() != null;

    private void Awake()
    {
        if (slotsRoot == null)
        {
            BuildDefaultLayout();
        }

        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        CacheWeaponSlots();
    }

    private void Start()
    {
        if (inventorySystem != null)
        {
            inventorySystem.SetSlots(FindObjectsOfType<InventorySlot>());
        }

        SelectSlot(0);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SelectSlot(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SelectSlot(1);
        }
    }

    private void CacheWeaponSlots()
    {
        weaponSlots.Clear();

        if (slotsRoot == null)
        {
            return;
        }

        var slots = slotsRoot.GetComponentsInChildren<InventorySlot>(true);
        weaponSlots.AddRange(slots);
    }

    private void SelectSlot(int index)
    {
        if (weaponSlots.Count == 0)
        {
            return;
        }

        if (index < 0 || index >= weaponSlots.Count)
        {
            return;
        }

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (i == index)
            {
                weaponSlots[i].Select();
            }
            else
            {
                weaponSlots[i].Deselect();
            }
        }

        selectedIndex = index;
    }

    private InventoryItem GetSelectedWeaponItem()
    {
        if (weaponSlots.Count == 0)
        {
            return null;
        }

        var selectedSlot = weaponSlots[Mathf.Clamp(selectedIndex, 0, weaponSlots.Count - 1)];
        var item = selectedSlot.GetComponentInChildren<InventoryItem>();
        if (item == null || item.itemObj == null)
        {
            return null;
        }

        return item.itemObj.category == InventoryItemObj.ItemCategory.Weapon ? item : null;
    }

    private void BuildDefaultLayout()
    {
        quickbarCanvas = CreateCanvas("WeaponQuickbarCanvas");
        slotsRoot = CreateSlotGroup("WeaponSlots", quickbarCanvas.transform as RectTransform, weaponSlotCount);
    }

    private Canvas CreateCanvas(string name)
    {
        var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");

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

    private RectTransform CreateSlotGroup(string name, RectTransform parent, int slotCount)
    {
        var groupObject = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup));
        groupObject.layer = LayerMask.NameToLayer("UI");
        groupObject.transform.SetParent(parent, false);

        var groupRect = groupObject.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(1f, 0f);
        groupRect.anchorMax = new Vector2(1f, 0f);
        groupRect.pivot = new Vector2(1f, 0f);
        groupRect.anchoredPosition = anchoredPosition;

        var grid = groupObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = slotSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 1;
        grid.childAlignment = TextAnchor.LowerRight;

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

        var image = slotObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.15f);

        var slotMarker = slotObject.AddComponent<InventorySlotMarker>();
        slotMarker.Setup(groupName, index);

        var slot = slotObject.AddComponent<InventorySlot>();
        slot.Configure(image, slotSelectedColor, slotDeselectedColor);
    }
}
