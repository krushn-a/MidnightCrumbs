using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Message System")]
    [SerializeField] private MessageManager messageManager;

    [Header("Game State")]
    [SerializeField] private bool startPaused = true;


    public static GameManager Instance { get; private set; }
    public Transform Player { get; private set; }


    private bool isGamePaused = false;
    private bool messagesCompleted = false;

    private void Awake()
    {
        Instance = this;
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Start()
    {
        if (startPaused)
        {
            PauseGame();
        }

        // Subscribe to message completion event
        if (messageManager != null)
        {
            messageManager.OnAllMessagesCompleted += OnMessagesCompleted;
        }
        else
        {
            Debug.LogWarning("MessageManager reference not assigned in GameManager!");
            // If no message manager, resume game immediately
            ResumeGame();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (messageManager != null)
        {
            messageManager.OnAllMessagesCompleted -= OnMessagesCompleted;
        }
    }

    private void OnMessagesCompleted()
    {
        Debug.Log("All messages completed. Resuming game...");
        messagesCompleted = true;
        ResumeGame();
    }

    public void PauseGame()
    {
        if (!isGamePaused)
        {
            Time.timeScale = 0f;
            isGamePaused = true;
            Debug.Log("Game paused - waiting for messages to complete");
            

            // Pause audio
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    public void ResumeGame()
    {
        if (isGamePaused)
        {
            Time.timeScale = 1f;
            isGamePaused = false;
            Debug.Log("Game resumed");
           

            // Resume audio
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    // Public methods for external control
    public bool IsGamePaused => isGamePaused;
    public bool AreMessagesCompleted => messagesCompleted;

    // Method to manually skip messages and resume game (for testing)
    public void ForceResumeGame()
    {
        if (messageManager != null)
        {
            messageManager.SkipAllMessages();
        }
        ResumeGame();
    }
}
