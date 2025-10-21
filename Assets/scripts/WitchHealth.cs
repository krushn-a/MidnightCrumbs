using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WitchHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthLossPerCookie = 10f;

    [Header("UI References")]
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthTrail;
    [SerializeField] private float trailLerpSpeed = 2f;

    [Header("Paralysis Settings")]
    [SerializeField] private float paralysisDuration = 120f;
    private bool isParalyzed = false;
    private float paralysisTimer = 0f;

    [Header("Gate References")]
    [SerializeField] private Animator leftGateAnimator;
    [SerializeField] private Animator rightGateAnimator;

    [Header("Witch References")]
    [SerializeField] private WitchAI witchAI;
    [SerializeField] private Animator witchAnimator;

    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindReferences = true;

    private float targetFillAmount;
    private bool hasEnteredGate = false;

    private void Awake()
    {
        if (autoFindReferences)
        {
            FindReferences();
        }

        currentHealth = maxHealth;
        targetFillAmount = 1f;
        UpdateHealthBar(true);
        HideHealthBar();
    }

    private void Update()
    {
        if (isParalyzed)
        {
            paralysisTimer -= Time.deltaTime;
            if (paralysisTimer <= 0f)
            {
                RecoverFromParalysis();
            }
        }

        if (healthTrail != null)
        {
            healthTrail.fillAmount = Mathf.Lerp(healthTrail.fillAmount, targetFillAmount, Time.deltaTime * trailLerpSpeed);
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateHealthBar(false);

        if (currentHealth <= 0f)
        {
            TriggerParalysisAndOpenGates();
        }
    }

    private void UpdateHealthBar(bool instant)
    {
        targetFillAmount = currentHealth / maxHealth;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = targetFillAmount;
        }

        if (healthTrail != null && instant)
        {
            healthTrail.fillAmount = targetFillAmount;
        }
    }

    private void TriggerParalysisAndOpenGates()
    {
        if (isParalyzed) return;

        isParalyzed = true;
        paralysisTimer = paralysisDuration;

        ParalyzeWitch();
        OpenCemeteryGates();
    }

    private void ParalyzeWitch()
    {
        if (witchAI != null)
        {
            witchAI.enabled = false;
        }

        if (witchAnimator != null)
        {
            witchAnimator.SetBool("isWalking", false);
            witchAnimator.SetBool("isFoundRunning", false);
        }

        Debug.Log("Witch paralyzed for 2 minutes!");
    }

    private void RecoverFromParalysis()
    {
        isParalyzed = false;
        currentHealth = maxHealth;
        UpdateHealthBar(true);

        if (witchAI != null)
        {
            witchAI.enabled = true;
        }

        Debug.Log("Witch recovered from paralysis!");
    }

    private void OpenCemeteryGates()
    {
        if (leftGateAnimator != null)
        {
            leftGateAnimator.SetTrigger("OpenLeftGate");
        }

        if (rightGateAnimator != null)
        {
            rightGateAnimator.SetTrigger("OpenRightGate");
        }

        Debug.Log("Cemetery gates opened!");
    }

    public void ShowHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
            hasEnteredGate = true;
            Debug.Log("Witch health bar revealed!");
        }
    }

    private void HideHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }
    }

    private void FindReferences()
    {
        if (witchAI == null)
        {
            witchAI = GetComponent<WitchAI>();
        }

        if (witchAnimator == null)
        {
            witchAnimator = GetComponent<Animator>();
        }

        if (healthBarUI == null)
        {
            healthBarUI = GameObject.Find("WitchSanity");
        }

        if (healthBarFill == null)
        {
            GameObject fillObj = GameObject.Find("WitchHealthBarFill");
            if (fillObj != null)
            {
                healthBarFill = fillObj.GetComponent<Image>();
            }
        }

        if (healthTrail == null)
        {
            GameObject trailObj = GameObject.Find("WitchHealthTrail");
            if (trailObj != null)
            {
                healthTrail = trailObj.GetComponent<Image>();
            }
        }

        if (leftGateAnimator == null || rightGateAnimator == null)
        {
            GameObject leftGate = GameObject.Find("Gate_3");
            GameObject rightGate = GameObject.Find("Gate_2");

            if (leftGate != null)
            {
                leftGateAnimator = leftGate.GetComponent<Animator>();
            }

            if (rightGate != null)
            {
                rightGateAnimator = rightGate.GetComponent<Animator>();
            }
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsParalyzed()
    {
        return isParalyzed;
    }

    public bool HasEnteredGate()
    {
        return hasEnteredGate;
    }
}
