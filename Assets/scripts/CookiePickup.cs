using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class CookiePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int amount = 1;
    public AudioClip pickupSfx;
    public GameObject pickupVfxPrefab; // optional particle system prefab

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
        // Find the player and their inventory
        var fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc == null) return;

        var inventory = fpc.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("CookiePickup: PlayerInventory not found on player.");
            return;
        }

        inventory.AddCookies(amount);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
