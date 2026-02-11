using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string playSceneName = "Main";
    [SerializeField] private string stashSceneName = "Inventory";

    [Header("UI")]
    [SerializeField] private Canvas targetCanvas;

    private const string MenuRootName = "MainMenuRoot";

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

        targetCanvas.overrideSorting = true;
        targetCanvas.sortingOrder = 100;

        if (targetCanvas.transform.Find(MenuRootName) == null)
        {
            BuildMenuUi(targetCanvas.transform);
        }
    }

    public void Play()
    {
        LoadSceneByName(playSceneName);
    }

    public void OpenStash()
    {
        LoadSceneByName(stashSceneName);
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

    private void BuildMenuUi(Transform parent)
    {
        var root = new GameObject(MenuRootName, typeof(RectTransform), typeof(VerticalLayoutGroup));
        root.transform.SetParent(parent, false);

        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(420f, 300f);

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 16f;
        layout.padding = new RectOffset(24, 24, 24, 24);

        CreateButton(root.transform, "Play", Play);
        CreateButton(root.transform, "Stash", OpenStash);
        CreateButton(root.transform, "Exit", Exit);
    }

    private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.45f, 0.45f, 0.45f, 1f);

        var layoutElement = buttonGo.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 64f;
        layoutElement.minHeight = 64f;

        var button = buttonGo.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(buttonGo.transform, false);

        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 42;
        text.enableAutoSizing = true;
        text.fontSizeMin = 20;
        text.fontSizeMax = 42;
    }
}
