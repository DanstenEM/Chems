using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
