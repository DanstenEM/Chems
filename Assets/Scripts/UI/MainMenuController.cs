using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string playSceneName = "SampleScene";
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
        var root = new GameObject(MenuRootName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        root.transform.SetParent(parent, false);

        var rootRect = (RectTransform)root.transform;
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(360f, 260f);

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 16f;
        layout.padding = new RectOffset(24, 24, 24, 24);

        var sizeFitter = root.GetComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateButton(root.transform, "Play", Play);
        CreateButton(root.transform, "Stash", OpenStash);
        CreateButton(root.transform, "Exit", Exit);
    }

    private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var image = buttonGo.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.92f);

        var layoutElement = buttonGo.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 56f;

        var button = buttonGo.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(buttonGo.transform, false);

        var textRect = (RectTransform)textGo.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.12f, 0.16f, 0.22f, 1f);
        text.fontSize = 32;
    }
}
