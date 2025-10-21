using UnityEngine;

public class InsideGateIndicator : MonoBehaviour
{
    [SerializeField] private Animator CloseLeftGate;
    [SerializeField] private Animator CloseRightGate;
    [SerializeField] private WitchHealth witchHealth;
    [SerializeField] private bool autoFindWitchHealth = true;

    private bool hasTriggered = false;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            CloseLeftGate.SetTrigger("CloseLeftGate");
            CloseRightGate.SetTrigger("CloseRightGate");

            if (witchHealth != null)
            {
                witchHealth.ShowHealthBar();
            }

            Debug.Log("Player entered gate - gates closed and witch health bar revealed!");
        }
    }
}
