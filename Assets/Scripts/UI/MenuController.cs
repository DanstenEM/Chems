using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string playSceneName = "Main";
    [SerializeField] private string stashSceneName = "Inventory";

    [Header("Input")]
    [SerializeField] private KeyCode playKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode stashKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode exitKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode alternateExitKey = KeyCode.Escape;

    [Header("Buttons")]
    [SerializeField] private TextMeshProUGUI playOption;
    [SerializeField] private TextMeshProUGUI stashOption;
    [SerializeField] private TextMeshProUGUI exitOption;
    [SerializeField] private Color buttonNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.55f);
    [SerializeField] private Color buttonHoverColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color buttonPressedColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);

    private void Start()
    {
        AutoAssignIfMissing();
        ConfigureButton(playOption, PlayGame);
        ConfigureButton(stashOption, OpenStash);
        ConfigureButton(exitOption, ExitGame);
    }

    private void AutoAssignIfMissing()
    {
        if (playOption == null)
        {
            playOption = FindMenuOption("PlayOption");
        }

        if (stashOption == null)
        {
            stashOption = FindMenuOption("StashOption");
        }

        if (exitOption == null)
        {
            exitOption = FindMenuOption("ExitOption");
        }
    }

    private static TextMeshProUGUI FindMenuOption(string optionName)
    {
        var option = GameObject.Find(optionName);
        return option != null ? option.GetComponent<TextMeshProUGUI>() : null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(playKey))
        {
            PlayGame();
            return;
        }

        if (Input.GetKeyDown(stashKey))
        {
            OpenStash();
            return;
        }

        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(alternateExitKey))
        {
            ExitGame();
        }
    }

    public void PlayGame()
    {
        LoadScene(playSceneName);
    }

    public void OpenStash()
    {
        LoadScene(stashSceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ConfigureButton(TextMeshProUGUI label, UnityEngine.Events.UnityAction onClick)
    {
        if (label == null)
        {
            return;
        }

        var buttonRoot = label.gameObject;

        var image = buttonRoot.GetComponent<Image>();
        if (image == null)
        {
            image = buttonRoot.AddComponent<Image>();
        }

        image.color = buttonNormalColor;

        var button = buttonRoot.GetComponent<Button>();
        if (button == null)
        {
            button = buttonRoot.AddComponent<Button>();
        }

        button.targetGraphic = image;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        var colors = button.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonHoverColor;
        colors.disabledColor = new Color(buttonNormalColor.r, buttonNormalColor.g, buttonNormalColor.b, 0.35f);
        button.colors = colors;
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
