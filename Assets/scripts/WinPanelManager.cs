using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindReferences = true;

    private void Awake()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }

        HideWinPanel();
        SetupButtons();
    }

    private void FindReferences()
    {
        if (winPanel == null)
        {
            winPanel = GameObject.Find("WinPanel");
        }

        if (playAgainButton == null)
        {
            GameObject playAgainObj = GameObject.Find("PlayAgain");
            if (playAgainObj != null)
            {
                playAgainButton = playAgainObj.GetComponent<Button>();
            }
        }

        if (mainMenuButton == null)
        {
            GameObject mainMenuObj = GameObject.Find("MainMenu");
            if (mainMenuObj != null)
            {
                mainMenuButton = mainMenuObj.GetComponent<Button>();
            }
        }

        if (quitButton == null)
        {
            GameObject quitObj = GameObject.Find("Quit");
            if (quitObj != null)
            {
                quitButton = quitObj.GetComponent<Button>();
            }
        }
    }

    private void SetupButtons()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(OnPlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuit);
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Win panel shown - Player escaped!");
    }

    private void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void OnPlayAgain()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Debug.Log("Restarting game");
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Returning to main menu");
    }

    private void OnQuit()
    {
        Debug.Log("Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
