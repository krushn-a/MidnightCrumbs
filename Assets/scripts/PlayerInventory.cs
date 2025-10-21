using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject Flashlight;

    [Header("Audio Source")]
    [SerializeField] private AudioClip FlashSound;
    private AudioSource FlashSoundSource;

    [Header("Cookies")]
    public int cookieCount = 0;
    // Bound to scene object named "CookieText"
    public TMP_Text cookieText;

    [Header("UI Binding")]
    public bool autoFindCookieText = true;
    public string cookieTextObjectName = "CookieText";
    public string cookieLabelFormat = "{0}"; // show just the number next to the cookie icon

    [Header("Witch Reaction")]
    public WitchAI witch;
    public WitchHealth witchHealth;
    public bool autoFindWitch = true;


    private void Update()
    {
        FlashLightToggle();
    }
    public void AddCookies(int amount)
    {
        if (amount == 0) return;
        cookieCount = Mathf.Max(0, cookieCount + amount);
        UpdateUI();

        EnsureWitch();
        EnsureWitchHealth();

        if (amount > 0)
        {
            if (witch != null)
            {
                for (int i = 0; i < amount; i++)
                    witch.IncreaseAggression();
            }

            if (witchHealth != null)
            {
                witchHealth.TakeDamage(10f * amount);
            }
        }
    }

    public void SetCookies(int value)
    {
        cookieCount = Mathf.Max(0, value);
        UpdateUI();
    }

    private void Awake()
    {
        if (autoFindWitch && witch == null)
            EnsureWitch();
        if (autoFindWitch && witchHealth == null)
            EnsureWitchHealth();
        UpdateUI();
        EnsureAudioSource();
    }

    private void EnsureWitch()
    {
        if (witch != null) return;
        // Prefer newer API; fallback if unavailable
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        witch = Object.FindFirstObjectByType<WitchAI>();
#else
        witch = FindObjectOfType<WitchAI>();
#endif
    }

    private void EnsureWitchHealth()
    {
        if (witchHealth != null) return;
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        witchHealth = Object.FindFirstObjectByType<WitchHealth>();
#else
    witchHealth = FindObjectOfType<WitchHealth>();
#endif
    }

    private void UpdateUI()
    {
        if (cookieText == null && autoFindCookieText)
        {
            //EnsureUIBinding();
        }
        if (cookieText != null)
        {
            cookieText.text = cookieCount.ToString();
        }
    }

    private void FlashLightToggle() 
    {
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            if (Flashlight.activeSelf)
            {
                Flashlight.SetActive(false);
                PlayFlashSound();
            }
            else
            {
                Flashlight.SetActive(true);
                PlayFlashSound();
            }
        }
    }

    private void EnsureAudioSource()
    {
        if (FlashSoundSource == null)
        {
            FlashSoundSource = GetComponent<AudioSource>();
            if (FlashSoundSource == null)
                FlashSoundSource = gameObject.AddComponent<AudioSource>();

            FlashSoundSource.playOnAwake = false;
            FlashSoundSource.spatialBlend = 0f; // 2D UI-like sound
        }
    }

    private void PlayFlashSound() 
    {
        if (FlashSound == null || FlashSoundSource == null) return;
        FlashSoundSource.PlayOneShot(FlashSound);
    }
}
