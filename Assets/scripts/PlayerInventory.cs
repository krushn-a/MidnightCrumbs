using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("Cookies")]
    public int cookieCount = 0;
    public TMP_Text cookieText; // UI is auto-created if not assigned

    [Header("UI Auto-Setup")]
    public bool autoCreateUI = true;
    public string uiTextObjectName = "CookiesText";
    public string cookieLabelFormat = "Cookies: {0}";

    [Header("Witch Reaction")]
    public WitchAI witch;            // assign in Inspector or auto-found
    public bool autoFindWitch = true;

    public void AddCookies(int amount)
    {
        if (amount == 0) return;
        cookieCount = Mathf.Max(0, cookieCount + amount);
        UpdateUI();

        // Increase witch aggression per cookie collected
        EnsureWitch();
        if (witch != null && amount > 0)
        {
            for (int i = 0; i < amount; i++)
                witch.IncreaseAggression();
        }
    }

    public void SetCookies(int value)
    {
        cookieCount = Mathf.Max(0, value);
        UpdateUI();
    }

    private void OnEnable()
    {
        EnsureUI();
        if (autoFindWitch && witch == null)
            EnsureWitch();
        UpdateUI();
    }

    private void EnsureUI()
    {
        if (cookieText != null || !autoCreateUI) return;

        // Create a minimal HUD
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("HUD");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        var textGO = new GameObject(uiTextObjectName);
        textGO.transform.SetParent(canvas.transform, false);
        var rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(15, -15);

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = Color.white;
        cookieText = tmp;
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

    private void UpdateUI()
    {
        if (cookieText != null)
        {
            cookieText.text = string.Format(cookieLabelFormat, cookieCount);
        }
    }
}
