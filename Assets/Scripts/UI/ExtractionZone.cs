using UnityEngine;
using TMPro;
<<<<<<< HEAD
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
=======
>>>>>>> parent of 461e4d5 (Merge pull request #32 from DanstenEM/codex/create-extraction-zone-prefab-with-trigger)

public class ExtractionZone : MonoBehaviour
{
    public float extractTime = 5f;
    public TMP_Text timerText;

<<<<<<< HEAD
    [Header("Overlay Defaults")]
    [SerializeField] private bool autoCreateOverlay = true;
    [SerializeField] private TMP_FontAsset fallbackFont;
    [SerializeField] private int overlayFontSize = 36;
    [SerializeField] private Color overlayTextColor = Color.white;
    [SerializeField] private Vector2 overlayOffset = new Vector2(0f, -60f);

    [Header("Activation")]
    [SerializeField] private DoorButtonInteractable doorButton;
    [SerializeField] private bool requireDoorOpen = true;
    [SerializeField] private string extractionSceneName;
    [SerializeField] private bool disablePlayerOnExtract = true;

=======
>>>>>>> parent of 461e4d5 (Merge pull request #32 from DanstenEM/codex/create-extraction-zone-prefab-with-trigger)
    float timer;
    bool playerInside;

    void Start()
    {
        if (timerText)
            timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInside) return;

        timer += Time.deltaTime;

        if (timerText)
        {
            float left = Mathf.Clamp(extractTime - timer, 0, extractTime);
            timerText.text = $"Extracting: {left:0.0}";
        }

        if (timer >= extractTime)
        {
            Extract();
        }
    }

    void Extract()
    {
        Debug.Log("EXTRACTION COMPLETE");

        if (timerText)
            timerText.gameObject.SetActive(false);

        enabled = false;
<<<<<<< HEAD
        HandleExtractionComplete();

#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#endif
    }

    private void HandleExtractionComplete()
    {
        if (!string.IsNullOrWhiteSpace(extractionSceneName))
        {
            SceneManager.LoadScene(extractionSceneName);
            return;
        }

        if (disablePlayerOnExtract)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                foreach (var behaviour in player.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (behaviour != this)
                    {
                        behaviour.enabled = false;
                    }
                }
            }
        }
    }

    private void HandleDoorOpened()
    {
        SetZoneActive(true);
    }

    private void SetZoneActive(bool active)
    {
        isActive = active;
        if (zoneCollider != null)
        {
            zoneCollider.enabled = active;
        }

        if (!active)
        {
            playerInside = false;
            timer = 0f;
            if (timerText)
            {
                timerText.gameObject.SetActive(false);
            }
        }
    }

    private void BuildOverlay()
    {
        var canvasObject = new GameObject("ExtractionOverlayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 50;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        overlayRoot = canvasObject.GetComponent<RectTransform>();
        overlayRoot.anchorMin = Vector2.zero;
        overlayRoot.anchorMax = Vector2.one;
        overlayRoot.offsetMin = Vector2.zero;
        overlayRoot.offsetMax = Vector2.zero;

        var textObject = new GameObject("ExtractionTimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(overlayRoot, false);

        var textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 1f);
        textTransform.anchorMax = new Vector2(0.5f, 1f);
        textTransform.pivot = new Vector2(0.5f, 1f);
        textTransform.anchoredPosition = overlayOffset;
        textTransform.sizeDelta = new Vector2(600f, 80f);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = overlayFontSize;
        text.color = overlayTextColor;
        if (fallbackFont != null)
        {
            text.font = fallbackFont;
        }

        timerText = text;
        timerText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (doorButton != null)
        {
            doorButton.DoorOpened -= HandleDoorOpened;
        }
=======
>>>>>>> parent of 461e4d5 (Merge pull request #32 from DanstenEM/codex/create-extraction-zone-prefab-with-trigger)
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(false);
    }
}