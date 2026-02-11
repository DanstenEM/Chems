using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string playSceneName = "Main";

    [Header("UI")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Stash Overlay")]
    [SerializeField] private int stashRows = 5;
    [SerializeField] private int stashColumns = 10;
    [SerializeField] private int inventoryRows = 5;
    [SerializeField] private int inventoryColumns = 5;

    [Header("Data Sources")]
    [SerializeField] private InventorySystem playerInventorySystem;

    private const string MenuRootName = "MainMenuRoot";

    private readonly List<ItemStackData> stashItems = new List<ItemStackData>();
    private readonly List<ItemStackData> playerItems = new List<ItemStackData>();
    private readonly List<TextMeshProUGUI> stashSlotTexts = new List<TextMeshProUGUI>();
    private readonly List<TextMeshProUGUI> playerSlotTexts = new List<TextMeshProUGUI>();

    private GameObject mainPanel;
    private GameObject stashPanel;

    private void Awake()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null)
        {
            Debug.LogError("MainMenuController could not find a Canvas in the scene.");
            return;
        }

        if (playerInventorySystem == null)
        {
            playerInventorySystem = InventorySystem.GameplayInventory;
            if (playerInventorySystem == null)
            {
                playerInventorySystem = FindObjectOfType<InventorySystem>();
            }
        }

        targetCanvas.overrideSorting = true;
        targetCanvas.sortingOrder = 100;

        if (targetCanvas.transform.Find(MenuRootName) == null)
        {
            BuildMenuUi(targetCanvas.transform);
        }

        RefreshInventoryMirror();
        RefreshAllSlotTexts();
    }

    public void Play()
    {
        LoadSceneByName(playSceneName);
    }

    public void OpenStash()
    {
        RefreshInventoryMirror();
        RefreshAllSlotTexts();
        SetStashVisible(true);
    }

    public void CloseStash()
    {
        SetStashVisible(false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SetStashVisible(bool isVisible)
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(!isVisible);
        }

        if (stashPanel != null)
        {
            stashPanel.SetActive(isVisible);
        }
    }

    private void BuildMenuUi(Transform parent)
    {
        var root = new GameObject(MenuRootName, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        mainPanel = BuildMainPanel(root.transform);
        stashPanel = BuildStashPanel(root.transform);

        SetStashVisible(false);
    }

    private GameObject BuildMainPanel(Transform parent)
    {
        var panel = new GameObject("MainPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(parent, false);

        var rect = (RectTransform)panel.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 300f);

        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 16f;
        layout.padding = new RectOffset(24, 24, 24, 24);

        CreateButton(panel.transform, "Play", Play);
        CreateButton(panel.transform, "Stash", OpenStash);
        CreateButton(panel.transform, "Exit", Exit);

        return panel;
    }

    private GameObject BuildStashPanel(Transform parent)
    {
        var panel = new GameObject("StashPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        var panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-24f, 0f);
        panelRect.sizeDelta = new Vector2(1520f, 640f);

        var panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        CreateText(panel.transform, "StashTitle", "Inventory Transfer", 48, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -80f), new Vector2(-24f, -16f), TextAlignmentOptions.Center);
        CreateText(panel.transform, "HelpText", "Click left slot to move item to stash. Click right slot to return item to inventory.", 22, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -126f), new Vector2(-24f, -64f), TextAlignmentOptions.Center);

        var leftSection = CreateSection(panel.transform, "PlayerInventorySection", "Player Inventory", new Vector2(0f, 0.5f), new Vector2(0.48f, 0.5f), new Vector2(740f, 460f), new Vector2(16f, -30f));
        var rightSection = CreateSection(panel.transform, "StashSection", "Stash", new Vector2(1f, 0.5f), new Vector2(0.52f, 0.5f), new Vector2(740f, 460f), new Vector2(-16f, -30f));

        BuildSlotGrid(leftSection, inventoryRows, inventoryColumns, playerSlotTexts, OnPlayerSlotClicked);
        BuildSlotGrid(rightSection, stashRows, stashColumns, stashSlotTexts, OnStashSlotClicked);

        CreateButton(panel.transform, "Back", CloseStash, new Vector2(220f, 64f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));

        return panel;
    }

    private RectTransform CreateSection(Transform parent, string name, string title, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
    {
        var section = new GameObject(name, typeof(RectTransform), typeof(Image));
        section.transform.SetParent(parent, false);

        var sectionRect = (RectTransform)section.transform;
        sectionRect.anchorMin = anchor;
        sectionRect.anchorMax = anchor;
        sectionRect.pivot = pivot;
        sectionRect.sizeDelta = size;
        sectionRect.anchoredPosition = anchoredPosition;

        var sectionImage = section.GetComponent<Image>();
        sectionImage.color = new Color(1f, 1f, 1f, 0.03f);

        CreateText(section.transform, "SectionTitle", title, 30, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -42f), new Vector2(-12f, -6f), TextAlignmentOptions.Center);

        var gridRoot = new GameObject("GridRoot", typeof(RectTransform));
        gridRoot.transform.SetParent(section.transform, false);

        var gridRect = (RectTransform)gridRoot.transform;
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.pivot = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(size.x - 24f, size.y - 96f);
        gridRect.anchoredPosition = new Vector2(0f, -14f);

        return gridRect;
    }

    private void BuildSlotGrid(RectTransform parent, int rows, int columns, List<TextMeshProUGUI> targetTextList, System.Action<int> onClick)
    {
        var gridObject = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObject.transform.SetParent(parent, false);

        var gridRect = (RectTransform)gridObject.transform;
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.pivot = new Vector2(0.5f, 0.5f);

        var safeRows = Mathf.Max(1, rows);
        var safeColumns = Mathf.Max(1, columns);

        var grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = safeColumns;
        grid.cellSize = new Vector2(66f, 66f);
        grid.spacing = new Vector2(6f, 6f);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;

        for (var row = 0; row < safeRows; row++)
        {
            for (var column = 0; column < safeColumns; column++)
            {
                var index = row * safeColumns + column;
                var text = CreateTransferSlot(gridObject.transform, index, onClick);
                targetTextList.Add(text);
            }
        }

        var width = (grid.cellSize.x * safeColumns) + (grid.spacing.x * (safeColumns - 1));
        var height = (grid.cellSize.y * safeRows) + (grid.spacing.y * (safeRows - 1));
        gridRect.sizeDelta = new Vector2(width, height);
    }

    private TextMeshProUGUI CreateTransferSlot(Transform parent, int index, System.Action<int> onClick)
    {
        var slotGo = new GameObject($"Slot_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
        slotGo.transform.SetParent(parent, false);

        var slotImage = slotGo.GetComponent<Image>();
        slotImage.color = new Color(0.65f, 0.65f, 0.65f, 0.45f);

        var button = slotGo.GetComponent<Button>();
        button.targetGraphic = slotImage;
        button.onClick.AddListener(() => onClick(index));

        return CreateText(slotGo.transform, "SlotText", "Empty", 14, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), TextAlignmentOptions.Center);
    }

    private void OnPlayerSlotClicked(int index)
    {
        if (index < 0 || index >= playerItems.Count)
        {
            return;
        }

        var item = playerItems[index];
        if (item == null || item.itemObj == null)
        {
            return;
        }

        if (playerInventorySystem != null && !playerInventorySystem.TryConsumeOne(item.itemObj))
        {
            return;
        }

        item.count -= 1;
        if (item.count <= 0)
        {
            playerItems.RemoveAt(index);
        }

        AddToList(stashItems, item.itemObj, 1);
        RefreshInventoryMirrorIfNeeded();
        RefreshAllSlotTexts();
    }

    private void OnStashSlotClicked(int index)
    {
        if (index < 0 || index >= stashItems.Count)
        {
            return;
        }

        var item = stashItems[index];
        if (item == null || item.itemObj == null)
        {
            return;
        }

        if (playerInventorySystem != null && !playerInventorySystem.AddItem(item.itemObj))
        {
            return;
        }

        item.count -= 1;
        if (item.count <= 0)
        {
            stashItems.RemoveAt(index);
        }

        AddToList(playerItems, item.itemObj, 1);
        RefreshInventoryMirrorIfNeeded();
        RefreshAllSlotTexts();
    }

    private void RefreshInventoryMirror()
    {
        playerItems.Clear();

        if (playerInventorySystem == null)
        {
            return;
        }

        var snapshot = playerInventorySystem.GetItemCounts();
        foreach (var pair in snapshot)
        {
            if (pair.Key == null || pair.Value <= 0)
            {
                continue;
            }

            playerItems.Add(new ItemStackData(pair.Key, pair.Value));
        }
    }

    private void RefreshInventoryMirrorIfNeeded()
    {
        if (playerInventorySystem == null)
        {
            return;
        }

        RefreshInventoryMirror();
    }

    private void AddToList(List<ItemStackData> targetList, InventoryItemObj itemObj, int amount)
    {
        if (itemObj == null || amount <= 0)
        {
            return;
        }

        foreach (var item in targetList)
        {
            if (item.itemObj != itemObj)
            {
                continue;
            }

            item.count += amount;
            return;
        }

        targetList.Add(new ItemStackData(itemObj, amount));
    }

    private void RefreshAllSlotTexts()
    {
        RefreshSlotTextList(playerSlotTexts, playerItems);
        RefreshSlotTextList(stashSlotTexts, stashItems);
    }

    private void RefreshSlotTextList(List<TextMeshProUGUI> texts, List<ItemStackData> source)
    {
        for (var i = 0; i < texts.Count; i++)
        {
            if (i < source.Count)
            {
                var item = source[i];
                texts[i].text = item.itemObj != null ? $"{item.itemObj.name}\nx{item.count}" : "Empty";
            }
            else
            {
                texts[i].text = "Empty";
            }
        }
    }

    private void CreateButton(
        Transform parent,
        string label,
        UnityEngine.Events.UnityAction onClick,
        Vector2? size = null,
        Vector2? anchorMin = null,
        Vector2? anchorMax = null,
        Vector2? anchoredPosition = null)
    {
        var buttonGo = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var rect = (RectTransform)buttonGo.transform;
        if (size.HasValue)
        {
            rect.sizeDelta = size.Value;
        }

        if (anchorMin.HasValue && anchorMax.HasValue)
        {
            rect.anchorMin = anchorMin.Value;
            rect.anchorMax = anchorMax.Value;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (anchoredPosition.HasValue)
        {
            rect.anchoredPosition = anchoredPosition.Value;
        }

        var image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.45f, 0.45f, 0.45f, 1f);

        var layoutElement = buttonGo.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 64f;
        layoutElement.minHeight = 64f;
        if (size.HasValue)
        {
            layoutElement.preferredWidth = size.Value.x;
            layoutElement.minWidth = size.Value.x;
        }

        var button = buttonGo.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateText(buttonGo.transform, "Label", label, 42, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, true);
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string label,
        int fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        TextAlignmentOptions alignment,
        bool autoSize = false)
    {
        var textGo = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.offsetMin = offsetMin;
        textRect.offsetMax = offsetMax;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = alignment;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.enableAutoSizing = autoSize;

        if (autoSize)
        {
            text.fontSizeMin = 12;
            text.fontSizeMax = fontSize;
        }

        return text;
    }

    private sealed class ItemStackData
    {
        public InventoryItemObj itemObj;
        public int count;

        public ItemStackData(InventoryItemObj itemObj, int count)
        {
            this.itemObj = itemObj;
            this.count = count;
        }
    }
}
