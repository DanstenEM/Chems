using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOverlay : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Health health;
    [SerializeField] private string healthLabel = "Health";

    [Header("Layout")]
    [SerializeField] private Vector2 panelPadding = new Vector2(18f, 12f);
    [SerializeField] private Vector2 panelOffset = new Vector2(24f, 24f);
    [SerializeField] private Vector2 panelSize = new Vector2(260f, 64f);
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 28;

    [Header("Scene References (Optional)")]
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake()
    {
        if (overlayRoot == null)
        {
            BuildDefaultLayout();
        }

        if (health == null)
        {
            health = FindObjectOfType<Health>();
        }

        UpdateHealthText();
    }

    private void Update()
    {
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText == null)
        {
            return;
        }

        if (health == null)
        {
            healthText.text = $"{healthLabel}: --";
            return;
        }

        healthText.text = $"{healthLabel}: {health.CurrentHealth:0}";
    }

    private void BuildDefaultLayout()
    {
        overlayCanvas = CreateCanvas("PlayerOverlayCanvas");
        overlayRoot = CreatePanel(overlayCanvas.transform, "PlayerOverlayPanel");
        healthText = CreateLabel(overlayRoot, "HealthText");
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

    private RectTransform CreatePanel(Transform parent, string name)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.layer = LayerMask.NameToLayer("UI");
        panelObject.transform.SetParent(parent, false);

        var rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.sizeDelta = panelSize;
        rectTransform.anchoredPosition = panelOffset;

        var image = panelObject.GetComponent<Image>();
        image.color = panelColor;

        return rectTransform;
    }

    private TextMeshProUGUI CreateLabel(RectTransform parent, string name)
    {
        var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.layer = LayerMask.NameToLayer("UI");
        labelObject.transform.SetParent(parent, false);

        var rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.offsetMin = new Vector2(panelPadding.x, panelPadding.y);
        rectTransform.offsetMax = new Vector2(-panelPadding.x, -panelPadding.y);

        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.color = textColor;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
        label.text = $"{healthLabel}: --";

        return label;
    }
}
