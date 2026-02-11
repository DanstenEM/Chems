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
    [SerializeField] private InventoryItemObj[] stashLookupFallbackItems;

    private const string MenuRootName = "MainMenuRoot";

    private readonly List<StashSlot> stashSlots = new List<StashSlot>();

    private GameObject mainPanel;
    private GameObject stashPanel;
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
        LoadSceneByName(playSceneName);
    }

    public void OpenStash()
    {
        RefreshStashView();
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
        panelRect.anchoredPosition = new Vector2(-40f, 0f);
        panelRect.sizeDelta = new Vector2(980f, 620f);

        var panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        CreateText(panel.transform, "StashTitle", "Stash", 52, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -88f), new Vector2(-24f, -16f), TextAlignmentOptions.Center);
        stashSubtitleText = CreateText(panel.transform, "StashSubtitle", "Loading stash...", 28, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -132f), new Vector2(-24f, -64f), TextAlignmentOptions.Center);

        var viewport = new GameObject("GridViewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(panel.transform, false);

        var viewportRect = (RectTransform)viewport.transform;
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.sizeDelta = new Vector2(900f, 420f);
        viewportRect.anchoredPosition = new Vector2(0f, -16f);

        var viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.04f);

        var viewportMask = viewport.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        var content = new GameObject("GridContent", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);

        var contentRect = (RectTransform)content.transform;
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, stashColumns);
        grid.cellSize = new Vector2(80f, 80f);
        grid.spacing = new Vector2(8f, 8f);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;

        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        stashSlots.Clear();
        var safeRows = Mathf.Max(1, stashRows);
        var safeColumns = Mathf.Max(1, stashColumns);

        for (var row = 0; row < safeRows; row++)
        {
            for (var column = 0; column < safeColumns; column++)
            {
                var slotIndex = row * safeColumns + column + 1;
                stashSlots.Add(CreateStashSlot(content.transform, slotIndex));
            }
        }

        CreateButton(panel.transform, "Back", CloseStash, new Vector2(220f, 64f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));

        return panel;
    }

    private StashSlot CreateStashSlot(Transform parent, int slotIndex)
    {
        var slotGo = new GameObject($"Slot_{slotIndex}", typeof(RectTransform), typeof(Image), typeof(StashSlot));
        slotGo.transform.SetParent(parent, false);

        var slotImage = slotGo.GetComponent<Image>();
        slotImage.color = new Color(0.65f, 0.65f, 0.65f, 0.28f);

        var stashSlot = slotGo.GetComponent<StashSlot>();
        stashSlot.Configure(slotImage);

        return stashSlot;
    }

    private void RefreshStashView()
    {
        if (stashSlots.Count == 0)
        {
            return;
        }

        stashItemLookup ??= InventorySnapshotMapper.BuildLookupWithFallbacks(
            InventorySnapshotMapper.BuildLookupFromResources(),
            stashLookupFallbackItems);

        foreach (var slot in stashSlots)
        {
            ClearChildren(slot.transform);
        }

        var stashInventory = InventoryPersistenceService.LoadStash();
        int renderedStacks = 0;

        bool stashModified = false;

        if (stashInventory != null && stashInventory.stacks != null)
        {
            var orderedStacks = new List<SavedItemStack>(stashInventory.stacks);
            orderedStacks.Sort((left, right) => string.Compare(left?.itemId, right?.itemId, System.StringComparison.Ordinal));
            var sanitizedStacks = new List<SavedItemStack>(orderedStacks.Count);

            foreach (var stack in orderedStacks)
            {
                if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.count <= 0)
                {
                    stashModified = true;
                    continue;
                }

                if (!stashItemLookup.TryGetValue(stack.itemId, out var itemObj) || itemObj == null)
                {
                    stashModified = true;
                    continue;
                }

                sanitizedStacks.Add(stack);

                if (renderedStacks >= stashSlots.Count)
                {
                    continue;
                }

                RenderItemStack(stashSlots[renderedStacks].transform, itemObj, stack.count, stack.itemId);
                renderedStacks++;
            }

            if (sanitizedStacks.Count > stashSlots.Count)
            {
                Debug.LogWarning("Stash has more valid stacks than available menu slots. Remaining stacks are hidden.");
            }

            if (stashModified || sanitizedStacks.Count != stashInventory.stacks.Count)
            {
                stashInventory.stacks = sanitizedStacks;
                InventoryPersistenceService.SaveStash(stashInventory);
            }
        }

        if (stashSubtitleText != null)
        {
            stashSubtitleText.text = renderedStacks == 0
                ? "No extracted loot yet"
                : $"Stored stacks: {renderedStacks}";
        }
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
        stashItem.Construct(itemId, itemObj.icon, GetCategoryColor(itemObj.category), count);
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
