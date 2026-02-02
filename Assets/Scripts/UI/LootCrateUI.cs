using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootCrateUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(500f, 360f);
    [SerializeField] private Vector2 itemSize = new Vector2(120f, 120f);
    [SerializeField] private Vector2 itemSpacing = new Vector2(12f, 12f);
    [SerializeField] private int slotCount = 15;
    [SerializeField] private int columns = 5;
    [SerializeField] private Vector2 panelOffset = new Vector2(40f, 0f);
    [SerializeField] private bool startHidden = true;

    [Header("Appearance")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color itemBackgroundColor = new Color(1f, 1f, 1f, 0.1f);

    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas lootCanvas;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private RectTransform itemsRoot;

    private readonly List<GameObject> createdObjects = new List<GameObject>();

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
    }

    public void Toggle(IReadOnlyList<InventoryItemObj> lootItems)
    {
        if (panelRoot == null)
        {
            return;
        }

        bool shouldShow = !panelRoot.gameObject.activeSelf;
        panelRoot.gameObject.SetActive(shouldShow);

        if (shouldShow)
        {
            Populate(lootItems);
        }
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

        itemsRoot = CreateItemsRoot(panelRoot, "LootItemsRoot");
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
        image.color = panelColor;

        return rectTransform;
    }

    private RectTransform CreateItemsRoot(Transform parent, string name)
    {
        var rootObject = new GameObject(name, typeof(RectTransform), typeof(GridLayoutGroup));
        rootObject.layer = LayerMask.NameToLayer("UI");
        rootObject.transform.SetParent(parent, false);
        createdObjects.Add(rootObject);

        var rectTransform = rootObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        var grid = rootObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = itemSize;
        grid.spacing = itemSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);
        grid.childAlignment = TextAnchor.UpperCenter;

        return rectTransform;
    }

    private void Populate(IReadOnlyList<InventoryItemObj> lootItems)
    {
        if (itemsRoot == null)
        {
            return;
        }

        for (int i = itemsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsRoot.GetChild(i).gameObject);
        }

        if (slotCount <= 0)
        {
            return;
        }

        int itemCount = lootItems != null ? lootItems.Count : 0;
        for (int i = 0; i < slotCount; i++)
        {
            if (i < itemCount)
            {
                CreateItemSlot(lootItems[i]);
            }
            else
            {
                CreateEmptySlot();
            }
        }
    }

    private void CreateEmptySlot()
    {
        var slotObject = CreateSlotRoot("EmptySlot");

        var label = slotObject.AddComponent<TextMeshProUGUI>();
        label.text = "Empty";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 20f;
        label.color = Color.white;
    }

    private void CreateItemSlot(InventoryItemObj item)
    {
        var slotObject = CreateSlotRoot(item != null ? item.name : "UnknownItem");

        if (item != null && item.icon != null)
        {
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.layer = LayerMask.NameToLayer("UI");
            iconObject.transform.SetParent(slotObject.transform, false);

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;

            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.3f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }

        var textObject = new GameObject("Name", typeof(RectTransform));
        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(slotObject.transform, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = item != null ? item.name : "Unknown";
        text.alignment = TextAlignmentOptions.Bottom;
        text.fontSize = 18f;
        text.color = Color.white;

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.05f);
        textRect.anchorMax = new Vector2(0.95f, 0.3f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private GameObject CreateSlotRoot(string name)
    {
        var slotObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        slotObject.layer = LayerMask.NameToLayer("UI");
        slotObject.transform.SetParent(itemsRoot, false);

        var image = slotObject.GetComponent<Image>();
        image.color = itemBackgroundColor;

        return slotObject;
    }
}
