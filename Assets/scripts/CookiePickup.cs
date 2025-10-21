using System;
using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class CookiePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int amount = 1;
    public AudioClip pickupSfx;
    public GameObject pickupVfxPrefab; // optional particle system prefab

    // Raised when the cookie is collected (before it is destroyed)
    public event Action<CookiePickup> Collected;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        // Ensure the cookie has the requested X rotation
        var e = transform.eulerAngles;
        e.x = -93.61f;
        transform.eulerAngles = e;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerInventory inventory = other.GetComponent<PlayerInventory>()
            ?? other.GetComponentInParent<PlayerInventory>()
            ?? other.GetComponentInChildren<PlayerInventory>();

        if (inventory == null)
        {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            inventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
#else
            inventory = FindObjectOfType<PlayerInventory>();
#endif
        }

        if (inventory == null)
        {
            Debug.LogWarning("CookiePickup: PlayerInventory not found on player or in scene.");
            return;
        }

        inventory.AddCookies(amount);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        // Notify listeners that this cookie has been collected
        Collected?.Invoke(this);

        Destroy(gameObject);
    }
}
