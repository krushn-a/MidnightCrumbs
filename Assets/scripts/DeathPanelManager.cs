using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;

    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindReferences = true;

    private void Awake()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }

        HideDeathPanel();
        SetupButtons();
    }

    private void FindReferences()
    {
        if (deathPanel == null)
        {
            deathPanel = GameObject.Find("DeathPanel");
        }

        if (playAgainButton == null)
        {
            GameObject playAgainObj = GameObject.Find("PlayAgain");
            if (playAgainObj != null)
            {
                playAgainButton = playAgainObj.GetComponent<Button>();
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

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuit);
        }
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Death panel shown - Game paused");
    }

    private void HideDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    private void OnPlayAgain()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Debug.Log("Restarting game");
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
