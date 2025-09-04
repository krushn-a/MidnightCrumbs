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

    //[Header("UI Auto-Setup")]
    //public bool autoCreateUI = true;
    //public string uiTextObjectName = "HealthText";
    //public string healthLabelFormat = "Health: {0}/{1}";
    //public TMP_Text healthText; // Auto-created if not assigned

    [Header("Audio")]
    public AudioClip hurtSfx;          // assign a clip from Assets/sounds
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
        UpdateUI();
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
        if (currentHealth == 0)
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
        // Optional: disable controls, trigger respawn, etc.
    }

    //private void EnsureUI()
    //{
    //    if (healthText != null || !autoCreateUI) return;

    //    // Create or reuse a Canvas for HUD
    //    var canvas = FindObjectOfType<Canvas>();
    //    if (canvas == null)
    //    {
    //        var cgo = new GameObject("HUD");
    //        canvas = cgo.AddComponent<Canvas>();
    //        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //        cgo.AddComponent<CanvasScaler>();
    //        cgo.AddComponent<GraphicRaycaster>();
    //    }

    //    var textGO = new GameObject(uiTextObjectName);
    //    textGO.transform.SetParent(canvas.transform, false);
    //    var rt = textGO.AddComponent<RectTransform>();
    //    rt.anchorMin = new Vector2(0, 1);
    //    rt.anchorMax = new Vector2(0, 1);
    //    rt.pivot = new Vector2(0, 1);
    //    rt.anchoredPosition = new Vector2(15, -50); // below cookies

    //    var tmp = textGO.AddComponent<TextMeshProUGUI>();
    //    tmp.fontSize = 28;
    //    tmp.alignment = TextAlignmentOptions.TopLeft;
    //    tmp.color = Color.red;
    //    healthText = tmp;
    //}

    private void EnsureAudioSource()
    {
        if (_audio == null)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();

            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f; // 2D UI-like sound
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
