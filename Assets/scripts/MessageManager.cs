using System;
using System.Collections;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [SerializeField] private GameObject[] MessageObject = new GameObject[3];
    [SerializeField] private float fadeDuration = 1.0f;

    private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private CanvasGroup[] panelCanvasGroup = new CanvasGroup[3];

    // Events
    public event EventHandler OnNextButtonPressed;
    public event Action OnAllMessagesCompleted;

    private bool isWaitingForInput = false;
    private bool inputReceived = false;
    private int currentMessageIndex = 0;
    private bool messagesCompleted = false;

    private void Awake()
    {
        InitializeMessages();
        StartCoroutine(ShowMessages());
    }

    private void Update()
    {
        // Handle input in Update for reliable detection
        if (isWaitingForInput && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed");
            inputReceived = true;
        }
    }

    private void InitializeMessages()
    {
        for (int i = 0; i < MessageObject.Length; i++)
        {
            if (MessageObject[i] != null)
            {
                MessageObject[i].SetActive(false);

                // Ensure CanvasGroup component exists
                panelCanvasGroup[i] = MessageObject[i].GetComponent<CanvasGroup>();
                if (panelCanvasGroup[i] == null)
                {
                    panelCanvasGroup[i] = MessageObject[i].AddComponent<CanvasGroup>();
                }

                panelCanvasGroup[i].alpha = 0f;
            }
        }
    }

    IEnumerator ShowMessages()
    {
        for (int i = 0; i < MessageObject.Length; i++)
        {
            if (MessageObject[i] != null)
            {
                currentMessageIndex = i;

                // Show and fade in current message
                yield return StartCoroutine(FadeInMessage(i));

                // Wait for input
                isWaitingForInput = true;
                inputReceived = false;

                yield return new WaitUntil(() => inputReceived);

                isWaitingForInput = false;

                // Trigger event
                OnNextButtonPressed?.Invoke(this, EventArgs.Empty);

                // Fade out current message
                yield return StartCoroutine(FadeOutMessage(i));
            }
        }

        Debug.Log("All messages completed");
        CompleteMessages();
    }

    private void CompleteMessages()
    {
        messagesCompleted = true;
        OnAllMessagesCompleted?.Invoke();
    }

    private IEnumerator FadeInMessage(int messageIndex)
    {
        MessageObject[messageIndex].SetActive(true);
        panelCanvasGroup[messageIndex].alpha = 0f;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime for paused game
            float normalizedTime = elapsedTime / fadeDuration;
            float curveValue = fadeInCurve.Evaluate(normalizedTime);

            panelCanvasGroup[messageIndex].alpha = curveValue;
            yield return null;
        }

        panelCanvasGroup[messageIndex].alpha = 1f;
    }

    private IEnumerator FadeOutMessage(int messageIndex)
    {
        float elapsedTime = 0f;
        float startAlpha = panelCanvasGroup[messageIndex].alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime for paused game
            float normalizedTime = elapsedTime / fadeDuration;
            float curveValue = fadeInCurve.Evaluate(1f - normalizedTime);

            panelCanvasGroup[messageIndex].alpha = startAlpha * curveValue;
            yield return null;
        }

        panelCanvasGroup[messageIndex].alpha = 0f;
        MessageObject[messageIndex].SetActive(false);
    }

    // Public method to manually trigger next message (for UI buttons)
    public void TriggerNext()
    {
        if (isWaitingForInput)
        {
            inputReceived = true;
        }
    }

    // Public method to skip all messages and complete immediately
    public void SkipAllMessages()
    {
        if (!messagesCompleted)
        {
            StopAllCoroutines();

            // Hide all messages
            for (int i = 0; i < MessageObject.Length; i++)
            {
                if (MessageObject[i] != null)
                {
                    MessageObject[i].SetActive(false);
                    panelCanvasGroup[i].alpha = 0f;
                }
            }

            CompleteMessages();
        }
    }

    // Public method to show a specific message
    public void ShowSpecificMessage(int index)
    {
        if (index >= 0 && index < MessageObject.Length && MessageObject[index] != null)
        {
            StopAllCoroutines();

            // Hide all messages first
            for (int i = 0; i < MessageObject.Length; i++)
            {
                if (MessageObject[i] != null)
                {
                    MessageObject[i].SetActive(false);
                    panelCanvasGroup[i].alpha = 0f;
                }
            }

            // Show specific message
            StartCoroutine(FadeInMessage(index));
        }
    }

    // Properties
    public bool AreMessagesCompleted => messagesCompleted;
    public int CurrentMessageIndex => currentMessageIndex;
}
