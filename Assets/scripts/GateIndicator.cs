using UnityEngine;

public class GateIndicator : MonoBehaviour
{
    [SerializeField] private Animator LeftGateAnimation;
    [SerializeField] private Animator RightGateAnimation;
    [SerializeField] private WitchHealth witchHealth;
    [SerializeField] private WinPanelManager winPanelManager;
    [SerializeField] private bool autoFindWitchHealth = true;
    [SerializeField] private bool autoFindWinPanel = true;

    private bool hasTriggeredWin = false;

    private void Awake()
    {
        if (autoFindWitchHealth && witchHealth == null)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            witchHealth = Object.FindFirstObjectByType<WitchHealth>();
#else
            witchHealth = FindObjectOfType<WitchHealth>();
#endif
        }

        if (autoFindWinPanel && winPanelManager == null)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            winPanelManager = Object.FindFirstObjectByType<WinPanelManager>();
#else
            winPanelManager = FindObjectOfType<WinPanelManager>();
#endif
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (witchHealth != null && witchHealth.HasEnteredGate() && witchHealth.GetCurrentHealth() > 0f)
            {
                Debug.Log("Gates remain closed - witch health is not zero!");
                return;
            }

            Debug.Log("Player approaching gates - Opening gates");
            LeftGateAnimation.SetTrigger("OpenLeftGate");
            RightGateAnimation.SetTrigger("OpenRightGate");

            if (witchHealth != null && witchHealth.HasEnteredGate() && witchHealth.GetCurrentHealth() <= 0f && !hasTriggeredWin)
            {
                hasTriggeredWin = true;

                if (winPanelManager != null)
                {
                    Invoke(nameof(ShowWinPanel), 1.5f);
                }
            }
        }
    }

    private void ShowWinPanel()
    {
        if (winPanelManager != null)
        {
            winPanelManager.ShowWinPanel();
        }
    }
}
