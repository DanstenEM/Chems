using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ExtractionZone : MonoBehaviour
{
    public float extractTime = 5f;
    public TMP_Text timerText;

    [Header("Overlay Defaults")]
    [SerializeField] private bool autoCreateOverlay = true;
    [SerializeField] private TMP_FontAsset fallbackFont;
    [SerializeField] private int overlayFontSize = 36;
    [SerializeField] private Color overlayTextColor = Color.white;
    [SerializeField] private Vector2 overlayOffset = new Vector2(0f, -60f);

    [Header("Activation")]
    [SerializeField] private DoorButtonInteractable doorButton;
    [SerializeField] private bool requireDoorOpen = true;

    [Header("Scene Flow")]
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private bool requireSuccessfulSaveBeforeSceneLoad = true;

    float timer;
    bool playerInside;
    bool isActive = true;
    Canvas overlayCanvas;
    RectTransform overlayRoot;
    Collider zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        if (doorButton != null)
        {
            doorButton.DoorOpened += HandleDoorOpened;
            if (requireDoorOpen)
            {
                SetZoneActive(false);
            }
        }

        if (timerText == null && autoCreateOverlay)
        {
            BuildOverlay();
        }
    }

    void Start()
    {
        if (timerText)
            timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;
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

        if (!TryPersistExtractedLoot())
        {
            if (requireSuccessfulSaveBeforeSceneLoad)
            {
                Debug.LogError("Extraction aborted because stash save failed.");
                enabled = true;
                playerInside = false;
                timer = 0f;
                return;
            }

            Debug.LogWarning("Stash save failed, but extraction will continue because strict save is disabled.");
        }

        enabled = false;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"Scene '{mainMenuSceneName}' is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private bool TryPersistExtractedLoot()
    {
        var gameplayInventory = InventorySystem.GameplayInventory;
        if (gameplayInventory == null)
        {
            Debug.LogWarning("Gameplay inventory not found. Saving empty post-extraction inventory payload.");
        }

        SavedInventory extractedSnapshot = InventorySnapshotMapper.BuildSnapshot(gameplayInventory);
        bool saved = InventoryPersistenceService.SavePostExtractionInventory(extractedSnapshot);
        if (saved)
        {
            int stackCount = extractedSnapshot != null && extractedSnapshot.stacks != null
                ? extractedSnapshot.stacks.Count
                : 0;
            Debug.Log($"Post-extraction inventory saved. Stack count: {stackCount}.");
        }

        return saved;
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        timer = 0f;

        if (timerText)
            timerText.gameObject.SetActive(false);
    }
}
