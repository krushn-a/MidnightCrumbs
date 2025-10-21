using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Image HealthBarFillImage;
    [SerializeField] private Image HealthBarTrailingImage;
    [SerializeField] private float HealthBarTrailDelay = 0.4f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Death Panel")]
    [SerializeField] private DeathPanelManager deathPanelManager;
    [SerializeField] private bool autoFindDeathPanel = true;

    [Header("Audio")]
    public AudioClip hurtSfx;
    public float hurtVolume = 1f;
    public float hurtSfxMinInterval = 0.15f;
    private AudioSource _audio;
    private float _nextHurtSfxTime;
    private float ratio = 1f;

    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        HealthBarFillImage.fillAmount = 1f;
        HealthBarTrailingImage.fillAmount = 1f;
        EnsureAudioSource();

        if (autoFindDeathPanel && deathPanelManager == null)
        {
            FindDeathPanelManager();
        }

        UpdateUI();
    }

    private void FindDeathPanelManager()
    {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        deathPanelManager = Object.FindFirstObjectByType<DeathPanelManager>();
#else
        deathPanelManager = FindObjectOfType<DeathPanelManager>();
#endif
    }

    public void SetMaxHealth(int value, bool refill = true)
    {
        maxHealth = Mathf.Max(1, value);
        if (refill) currentHealth = maxHealth;
        else currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || !IsAlive) return;
        currentHealth = currentHealth - amount;
        ratio = currentHealth / maxHealth;
        PlayHurtSfx();
        UpdateUI();
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

    private void OnDeath()
    {
        Debug.Log("Player died.");

        if (deathPanelManager != null)
        {
            deathPanelManager.ShowDeathPanel();
        }
        else
        {
            Debug.LogWarning("DeathPanelManager not found!");
        }
    }

    private void EnsureAudioSource()
    {
        if (_audio == null)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();

            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
        }
    }

    private void PlayHurtSfx()
    {
        if (hurtSfx == null || _audio == null) return;
        if (Time.time < _nextHurtSfxTime) return;

        _audio.PlayOneShot(hurtSfx, Mathf.Clamp01(hurtVolume));
        _nextHurtSfxTime = Time.time + hurtSfxMinInterval;
    }

    private void UpdateUI()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(HealthBarFillImage.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        seq.AppendInterval(HealthBarTrailDelay);
        seq.Append(HealthBarTrailingImage.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);

        seq.Play();
    }
}
