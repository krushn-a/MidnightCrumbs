using UnityEngine;
using TMPro; // for TextMeshPro UI
using StarterAssets; // to detect FirstPersonController

public class cauldron : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRadius = 0f; // optional if using trigger collider only

    [Header("Spawning")]
    public GameObject cookiePrefab;
    public Transform spawnPoint; // if null, uses cauldron's position
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    public Vector3 launchImpulse = new Vector3(0, 2f, 0); // optional pop-out

    [Header("Multiple Spawn Points")]
    public Transform[] spawnPoints; // Optional: if set, cookies spawn from these
    public bool randomizeSpawnPoint = true;
    private int _nextSpawnIndex = 0;

    [Header("Cooldown")]
    public float cooldownSeconds = 2f;
    private float _cooldownUntil = 0f;

    [Header("UI Prompt")]
    public TMP_Text promptText; // "Press E to collect"

    private bool _playerInRange;

    [Header("Spawn Avoidance")]
    public float spawnClearRadius = 0.35f;   // space required to consider a spot free
    public float spawnJitterRadius = 0.4f;   // random horizontal jitter if base spot is taken
    public int maxSpawnAttempts = 12;        // tries across points + jitter

    private void Reset()
    {
        // Try to auto-assign a child named "SpawnPoint"
        if (spawnPoint == null)
        {
            var t = transform.Find("SpawnPoint");
            if (t != null) spawnPoint = t;
        }

        // Ensure we have a trigger collider for interaction
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Try to auto-assign a parent named "SpawnPoints" and gather its children
        if ((spawnPoints == null || spawnPoints.Length == 0))
        {
            var parent = transform.Find("SpawnPoints");
            if (parent != null)
            {
                int childCount = parent.childCount;
                if (childCount > 0)
                {
                    spawnPoints = new Transform[childCount];
                    for (int i = 0; i < childCount; i++)
                        spawnPoints[i] = parent.GetChild(i);
                }
            }
        }
    }

    private void OnEnable()
    {
        SetPrompt(false);
        _cooldownUntil = 0f;
    }

    private void Update()
    {
        if (_playerInRange)
        {
            UpdatePrompt();

            if (Input.GetKeyDown(interactKey) && IsReady())
            {
                SpawnCookie();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerInRange = true;
            SetPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerInRange = false;
            SetPrompt(false);
        }
    }

    private bool IsPlayer(Collider other)
    {
        // Detect by component or tag
        return other.GetComponentInParent<FirstPersonController>() != null || other.CompareTag("Player");
    }

    private void SetPrompt(bool visible)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(visible);
    }

    private void UpdatePrompt()
    {
        if (promptText == null) return;
        if (IsReady())
        {
            promptText.text = $"Press {interactKey} to brew cookie";
        }
        else
        {
            float remaining = Mathf.Max(0f, _cooldownUntil - Time.time);
            promptText.text = $"Cooling down: {remaining:F1}s";
        }
    }

    private bool IsReady() => Time.time >= _cooldownUntil;

    private void SpawnCookie()
    {
        if (cookiePrefab == null)
        {
            Debug.LogWarning("Cauldron: cookiePrefab not assigned.");
            return;
        }

        Vector3 pos; Quaternion rot;
        if (!TryGetFreeSpawnPose(out pos, out rot))
        {
            Debug.Log("Cauldron: No free spawn spot found (all occupied).");
            return;
        }

        GameObject cookie = Instantiate(cookiePrefab, pos, rot);

        // Optional little pop-up impulse if there is a rigidbody
        if (cookie.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(launchImpulse, ForceMode.Impulse);
        }

        // Start cooldown
        if (cooldownSeconds > 0f)
            _cooldownUntil = Time.time + cooldownSeconds;

        // Hide prompt after spawning (optional)
        SetPrompt(false);
        _playerInRange = false;
    }

    private Transform ChooseSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        // Filter out nulls
        int validCount = 0;
        for (int i = 0; i < spawnPoints.Length; i++)
            if (spawnPoints[i] != null) validCount++;
        if (validCount == 0) return null;

        if (randomizeSpawnPoint)
        {
            // Random among non-null entries
            int tries = 0;
            while (tries < 10)
            {
                int idx = Random.Range(0, spawnPoints.Length);
                if (spawnPoints[idx] != null) return spawnPoints[idx];
                tries++;
            }
            // fallback sequential scan
            for (int i = 0; i < spawnPoints.Length; i++)
                if (spawnPoints[i] != null) return spawnPoints[i];
            return null;
        }
        else
        {
            // Round-robin among non-null entries
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                _nextSpawnIndex = (_nextSpawnIndex + 1) % spawnPoints.Length;
                if (spawnPoints[_nextSpawnIndex] != null)
                    return spawnPoints[_nextSpawnIndex];
            }
            return null;
        }
    }

    private bool TryGetFreeSpawnPose(out Vector3 pos, out Quaternion rot)
    {
        // Build candidate list (multiple spawn points > single point > cauldron)
        Transform[] candidates = (spawnPoints != null && spawnPoints.Length > 0) ? spawnPoints : null;
        Transform fallback = spawnPoint;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Transform baseT = null;
            if (candidates != null && candidates.Length > 0)
            {
                // Random candidate among non-null entries
                for (int tries = 0; tries < 10 && baseT == null; tries++)
                {
                    int idx = Random.Range(0, candidates.Length);
                    baseT = candidates[idx];
                }
            }
            if (baseT == null) baseT = fallback; // may be null

            Vector3 basePos = baseT != null ? baseT.position : transform.position + spawnOffset;
            Quaternion baseRot = baseT != null ? baseT.rotation : Quaternion.identity;

            // Jitter to avoid stacking if occupied
            Vector3 candidatePos = basePos;
            if (attempt > 0 && spawnJitterRadius > 0f)
            {
                var c = Random.insideUnitCircle * spawnJitterRadius;
                candidatePos += new Vector3(c.x, 0f, c.y);
            }

            if (!IsOccupied(candidatePos, spawnClearRadius))
            {
                pos = candidatePos;
                rot = baseRot;
                return true;
            }
        }

        pos = Vector3.zero; rot = Quaternion.identity;
        return false;
    }

    private bool IsOccupied(Vector3 position, float radius)
    {
        var hits = Physics.OverlapSphere(position, radius, ~0, QueryTriggerInteraction.Collide);
        foreach (var h in hits)
        {
            if (h == null) continue;
            // Only care about other cookies occupying the space
            if (h.GetComponentInParent<CookiePickup>() != null)
                return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionRadius > 0f)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
