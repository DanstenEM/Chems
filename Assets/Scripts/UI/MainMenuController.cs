using System;
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
    [SerializeField] private int extractedRegularSlots = 15;
    [SerializeField] private int extractedChemicalSlots = 4;
    [SerializeField] private int extractedWeaponSlots = 2;
    [SerializeField] private int stashRows = 5;
    [SerializeField] private int stashColumns = 5;
    [SerializeField] private InventoryItemObj[] stashLookupFallbackItems;

    private const string MenuRootName = "MainMenuRoot";

    private readonly List<StashSlot> extractedSlots = new List<StashSlot>();
    private readonly List<StashSlot> stashSlots = new List<StashSlot>();

    private GameObject mainPanel;
    private GameObject stashPanel;
    private TextMeshProUGUI extractedSubtitleText;
    private TextMeshProUGUI stashSubtitleText;
    private IReadOnlyDictionary<string, InventoryItemObj> stashItemLookup;

    private void Awake()
    {
        EnsureCursorIsInteractive();

        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null)
        {
            Debug.LogError("MainMenuController could not find a Canvas in the scene.");
            return;
        }

        targetCanvas.overrideSorting = true;
        targetCanvas.sortingOrder = 100;

        if (targetCanvas.transform.Find(MenuRootName) == null)
        {
            BuildMenuUi(targetCanvas.transform);
        }

        RefreshStashView();
    }

    private void EnsureCursorIsInteractive()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Play()
    {
        PersistStashView();
        LoadSceneByName(playSceneName);
    }

    public void OpenStash()
    {
        RefreshStashView();
        SetStashVisible(true);
    }

    public void CloseStash()
    {
        PersistStashView();
        SetStashVisible(false);
    }

    public void ClearStash()
    {
        InventoryPersistenceService.ClearStash();
        RefreshStashView();
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
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1600f, 760f);

        var panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        CreateText(panel.transform, "StashTitle", "Stash Management", 52, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -88f), new Vector2(-24f, -16f), TextAlignmentOptions.Center);

        var columns = new GameObject("Columns", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        columns.transform.SetParent(panel.transform, false);

        var columnsRect = (RectTransform)columns.transform;
        columnsRect.anchorMin = new Vector2(0f, 0f);
        columnsRect.anchorMax = new Vector2(1f, 1f);
        columnsRect.offsetMin = new Vector2(24f, 120f);
        columnsRect.offsetMax = new Vector2(-24f, -110f);

        var columnsLayout = columns.GetComponent<HorizontalLayoutGroup>();
        columnsLayout.childAlignment = TextAnchor.UpperCenter;
        columnsLayout.childControlHeight = true;
        columnsLayout.childControlWidth = true;
        columnsLayout.childForceExpandWidth = true;
        columnsLayout.childForceExpandHeight = true;
        columnsLayout.spacing = 16f;

        BuildExtractedInventoryColumn(columns.transform, "ExtractedInventory", out extractedSubtitleText);
        BuildInventoryColumn(columns.transform, "StashInventory", "Stash", "Loading stash...", stashSlots, Mathf.Max(1, stashRows), Mathf.Max(1, stashColumns), out stashSubtitleText, StashSlot.SlotFilter.Universal);

        CreateButton(panel.transform, "Back", CloseStash, new Vector2(220f, 64f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 36f));
        CreateButton(panel.transform, "Clear Stash", ClearStash, new Vector2(280f, 64f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 36f));

        return panel;
    }

    private void BuildExtractedInventoryColumn(
        Transform parent,
        string objectName,
        out TextMeshProUGUI subtitleText)
    {
        var column = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        column.transform.SetParent(parent, false);

        var columnImage = column.GetComponent<Image>();
        columnImage.color = new Color(1f, 1f, 1f, 0.04f);

        var columnLayout = column.GetComponent<VerticalLayoutGroup>();
        columnLayout.padding = new RectOffset(16, 16, 16, 16);
        columnLayout.spacing = 10f;
        columnLayout.childAlignment = TextAnchor.UpperCenter;
        columnLayout.childControlHeight = true;
        columnLayout.childControlWidth = true;
        columnLayout.childForceExpandHeight = false;
        columnLayout.childForceExpandWidth = true;

        CreateText(column.transform, $"{objectName}_Title", "Extracted Inventory", 34, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
        subtitleText = CreateText(column.transform, $"{objectName}_Subtitle", "Loading extracted loot...", 24, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        extractedSlots.Clear();
        BuildExtractedGroup(column.transform, "Regular", StashSlot.SlotFilter.Regular, Mathf.Max(1, extractedRegularSlots), 5, extractedSlots);
        BuildExtractedGroup(column.transform, "Chemical", StashSlot.SlotFilter.Chemical, Mathf.Max(1, extractedChemicalSlots), 4, extractedSlots);
        BuildExtractedGroup(column.transform, "Weapon", StashSlot.SlotFilter.Weapon, Mathf.Max(1, extractedWeaponSlots), 2, extractedSlots);
    }

    private void BuildExtractedGroup(
        Transform parent,
        string groupName,
        StashSlot.SlotFilter filter,
        int slotCount,
        int columns,
        List<StashSlot> targetSlots)
    {
        var groupContainer = new GameObject($"{groupName}Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
        groupContainer.transform.SetParent(parent, false);

        var groupLayout = groupContainer.GetComponent<VerticalLayoutGroup>();
        groupLayout.spacing = 8f;
        groupLayout.childAlignment = TextAnchor.UpperCenter;
        groupLayout.childControlHeight = true;
        groupLayout.childControlWidth = true;
        groupLayout.childForceExpandHeight = false;
        groupLayout.childForceExpandWidth = true;

        CreateText(groupContainer.transform, $"{groupName}Label", groupName, 24, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        var content = new GameObject($"{groupName}GridContent", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        content.transform.SetParent(groupContainer.transform, false);

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);
        grid.cellSize = new Vector2(80f, 80f);
        grid.spacing = new Vector2(8f, 8f);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;

        int rows = Mathf.CeilToInt(slotCount / (float)Mathf.Max(1, columns));
        float width = grid.constraintCount * grid.cellSize.x + Mathf.Max(0, grid.constraintCount - 1) * grid.spacing.x;
        float height = rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;

        var layoutElement = content.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = height;

        for (int i = 0; i < slotCount; i++)
        {
            targetSlots.Add(CreateStashSlot(content.transform, i + 1, filter));
        }
    }

    private void BuildInventoryColumn(
        Transform parent,
        string objectName,
        string title,
        string subtitle,
        List<StashSlot> targetSlots,
        int rows,
        int columns,
        out TextMeshProUGUI subtitleText,
        StashSlot.SlotFilter slotFilter)
    {
        var column = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        column.transform.SetParent(parent, false);

        var columnImage = column.GetComponent<Image>();
        columnImage.color = new Color(1f, 1f, 1f, 0.04f);

        var columnLayout = column.GetComponent<VerticalLayoutGroup>();
        columnLayout.padding = new RectOffset(16, 16, 16, 16);
        columnLayout.spacing = 10f;
        columnLayout.childAlignment = TextAnchor.UpperCenter;
        columnLayout.childControlHeight = true;
        columnLayout.childControlWidth = true;
        columnLayout.childForceExpandHeight = false;
        columnLayout.childForceExpandWidth = true;

        CreateText(column.transform, $"{objectName}_Title", title, 34, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
        subtitleText = CreateText(column.transform, $"{objectName}_Subtitle", subtitle, 24, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        var viewport = new GameObject($"{objectName}_GridViewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(LayoutElement));
        viewport.transform.SetParent(column.transform, false);

        var viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);

        var viewportMask = viewport.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        var viewportElement = viewport.GetComponent<LayoutElement>();
        viewportElement.flexibleHeight = 1f;
        viewportElement.preferredHeight = 500f;

        var content = new GameObject($"{objectName}_GridContent", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);

        var contentRect = (RectTransform)content.transform;
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(80f, 80f);
        grid.spacing = new Vector2(8f, 8f);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;

        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        targetSlots.Clear();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int slotIndex = row * columns + col + 1;
                targetSlots.Add(CreateStashSlot(content.transform, slotIndex, slotFilter));
            }
        }
    }

    private StashSlot CreateStashSlot(Transform parent, int slotIndex, StashSlot.SlotFilter filter)
    {
        var slotGo = new GameObject($"Slot_{slotIndex}", typeof(RectTransform), typeof(Image), typeof(StashSlot));
        slotGo.transform.SetParent(parent, false);

        var slotImage = slotGo.GetComponent<Image>();
        slotImage.color = new Color(0.65f, 0.65f, 0.65f, 0.28f);

        var stashSlot = slotGo.GetComponent<StashSlot>();
        stashSlot.Configure(slotImage, filter);

        return stashSlot;
    }

    private void RefreshStashView()
    {
        if (extractedSlots.Count == 0 || stashSlots.Count == 0)
        {
            return;
        }

        stashItemLookup ??= InventorySnapshotMapper.BuildLookupWithFallbacks(
            InventorySnapshotMapper.BuildLookupFromResources(),
            stashLookupFallbackItems);

        RenderSnapshotToSlots(InventoryPersistenceService.LoadPostExtractionInventory(), extractedSlots, extractedSubtitleText, "No extracted loot", "Extracted stacks: {0}");
        RenderSnapshotToSlots(InventoryPersistenceService.LoadStash(), stashSlots, stashSubtitleText, "No stash loot", "Stored stacks: {0}");
    }

    private void PersistStashView()
    {
        if (extractedSlots.Count == 0 || stashSlots.Count == 0)
        {
            return;
        }

        bool extractedSaved = InventoryPersistenceService.SavePostExtractionInventory(BuildSnapshotFromSlots(extractedSlots));
        bool stashSaved = InventoryPersistenceService.SaveStash(BuildSnapshotFromSlots(stashSlots));

        if (!extractedSaved || !stashSaved)
        {
            Debug.LogError("Failed to persist stash management inventories.");
        }
    }

    private void RenderSnapshotToSlots(
        SavedInventory snapshot,
        List<StashSlot> targetSlots,
        TextMeshProUGUI subtitleTarget,
        string emptyLabel,
        string countLabelFormat)
    {
        foreach (var slot in targetSlots)
        {
            if (slot != null)
            {
                ClearChildren(slot.transform);
            }
        }

        int renderedStacks = 0;
        if (snapshot != null && snapshot.stacks != null)
        {
            var sanitizedStacks = new List<SavedItemStack>(snapshot.stacks.Count);
            foreach (var stack in snapshot.stacks)
            {
                if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.count <= 0)
                {
                    continue;
                }

                if (!stashItemLookup.TryGetValue(stack.itemId, out var itemObj) || itemObj == null)
                {
                    continue;
                }

                sanitizedStacks.Add(stack);
            }

            sanitizedStacks.Sort((left, right) => string.Compare(left?.itemId, right?.itemId, StringComparison.Ordinal));

            foreach (var stack in sanitizedStacks)
            {
                if (!stashItemLookup.TryGetValue(stack.itemId, out var itemObj) || itemObj == null)
                {
                    continue;
                }

                int availableSlotIndex = FindNextCompatibleEmptySlotIndex(targetSlots, 0, itemObj.category);
                if (availableSlotIndex < 0)
                {
                    Debug.LogWarning("Inventory has more valid stacks than available menu slots. Remaining stacks are hidden.");
                    continue;
                }

                RenderItemStack(targetSlots[availableSlotIndex].transform, itemObj, stack.count, stack.itemId);
                renderedStacks++;
            }
        }

        if (subtitleTarget != null)
        {
            subtitleTarget.text = renderedStacks == 0
                ? emptyLabel
                : string.Format(countLabelFormat, renderedStacks);
        }
    }

    private static int FindNextCompatibleEmptySlotIndex(IReadOnlyList<StashSlot> slots, int startIndex, InventoryItemObj.ItemCategory category)
    {
        if (slots == null)
        {
            return -1;
        }

        int safeStartIndex = Mathf.Max(0, startIndex);
        for (int i = safeStartIndex; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null || slot.GetComponentInChildren<StashMenuItem>() != null)
            {
                continue;
            }

            if (slot.IsCategoryAllowed(category))
            {
                return i;
            }
        }

        for (int i = 0; i < safeStartIndex && i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null || slot.GetComponentInChildren<StashMenuItem>() != null)
            {
                continue;
            }

            if (slot.IsCategoryAllowed(category))
            {
                return i;
            }
        }

        return -1;
    }

    private static SavedInventory BuildSnapshotFromSlots(IEnumerable<StashSlot> sourceSlots)
    {
        var snapshot = new SavedInventory();
        var countByItemId = new Dictionary<string, int>();

        if (sourceSlots == null)
        {
            return snapshot;
        }

        foreach (var slot in sourceSlots)
        {
            if (slot == null)
            {
                continue;
            }

            var item = slot.GetComponentInChildren<StashMenuItem>();
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Count <= 0)
            {
                continue;
            }

            if (!countByItemId.TryAdd(item.ItemId, item.Count))
            {
                countByItemId[item.ItemId] += item.Count;
            }
        }

        foreach (var pair in countByItemId)
        {
            snapshot.stacks.Add(new SavedItemStack
            {
                itemId = pair.Key,
                count = pair.Value
            });
        }

        return snapshot;
    }

    private void RenderItemStack(Transform slotTransform, InventoryItemObj itemObj, int count, string itemId)
    {
        var itemVisual = new GameObject("ItemVisual", typeof(RectTransform), typeof(Image), typeof(StashMenuItem));
        itemVisual.transform.SetParent(slotTransform, false);

        var itemRect = (RectTransform)itemVisual.transform;
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = Vector2.one;
        itemRect.offsetMin = Vector2.zero;
        itemRect.offsetMax = Vector2.zero;

        var countGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        countGo.transform.SetParent(itemVisual.transform, false);

        var countRect = (RectTransform)countGo.transform;
        countRect.anchorMin = new Vector2(1f, 0f);
        countRect.anchorMax = new Vector2(1f, 0f);
        countRect.pivot = new Vector2(1f, 0f);
        countRect.anchoredPosition = new Vector2(-4f, 4f);
        countRect.sizeDelta = new Vector2(72f, 28f);

        var countText = countGo.GetComponent<TextMeshProUGUI>();
        countText.alignment = TextAlignmentOptions.BottomRight;
        countText.fontSize = 24;
        countText.color = Color.white;

        var itemImage = itemVisual.GetComponent<Image>();
        var stashItem = itemVisual.GetComponent<StashMenuItem>();
        stashItem.SetupComponents(itemImage, countText);
        var category = itemObj != null ? itemObj.category : InventoryItemObj.ItemCategory.Regular;
        stashItem.Construct(itemId, itemObj != null ? itemObj.icon : null, itemObj != null ? GetCategoryColor(itemObj.category) : new Color(1f, 0.85f, 0.2f, 1f), count, category);
    }

    private static Color GetCategoryColor(InventoryItemObj.ItemCategory category)
    {
        return category switch
        {
            InventoryItemObj.ItemCategory.Chemical => new Color(0.2f, 0.9f, 0.2f, 1f),
            InventoryItemObj.ItemCategory.Weapon => new Color(0.95f, 0.2f, 0.2f, 1f),
            _ => new Color(1f, 0.85f, 0.2f, 1f)
        };
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
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
            text.fontSizeMin = 20;
            text.fontSizeMax = fontSize;
        }

        return text;
    }
}
