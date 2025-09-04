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

    [Header("Batch Spawn")]
    [Tooltip("How many cookies to spawn each time you press the interact key.")]
    public int cookiesPerInteract = 3;
    [Tooltip("How many distinct spawn points to randomly select from the array when spawning a batch.")]
    public int spawnPointsToUse = 2;
    [Tooltip("Distribute cookies evenly across the selected spawn points (round-robin). If false, pick a random selected point for each cookie.")]
    public bool distributeAcrossSelected = true;

    [Header("Multiple Spawn Points")]
    public Transform[] spawnPoints; // Optional: if set, cookies spawn from these
    public bool randomizeSpawnPoint = true;
    private int _nextSpawnIndex = 0;

    [Header("Cooldown")]
    public float cooldownSeconds = 2f;
    private float _cooldownUntil = 0f;
    private int _outstandingBatchCookies = 0;

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
                SpawnCookiesBatch();
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
        if (!IsBatchComplete())
        {
            promptText.text = $"Collect all cookies first ({_outstandingBatchCookies} left)";
        }
        else if (IsReady())
        {
            int n = Mathf.Max(1, cookiesPerInteract);
            string label = n == 1 ? "cookie" : "cookies";
            promptText.text = $"Press {interactKey} to brew {n} {label}";
        }
        else
        {
            float remaining = Mathf.Max(0f, _cooldownUntil - Time.time);
            promptText.text = $"Cooling down: {remaining:F1}s";
        }
    }

    private bool IsReady() => Time.time >= _cooldownUntil && IsBatchComplete();
    private bool IsBatchComplete() => _outstandingBatchCookies <= 0;

    private void SpawnCookiesBatch()
    {
        if (cookiePrefab == null)
        {
            Debug.LogWarning("Cauldron: cookiePrefab not assigned.");
            return;
        }

    int count = Mathf.Max(1, cookiesPerInteract);
    _outstandingBatchCookies = 0; // reset and recount real spawns

        // Prepare a random subset of spawn points (distinct) if configured
        Transform[] selected = null;
        int selectedCount = 0;
        if (spawnPoints != null && spawnPoints.Length > 0 && spawnPointsToUse > 0)
        {
            selected = ChooseDistinctSpawnPoints(spawnPoints, spawnPointsToUse, out selectedCount);
        }

        for (int i = 0; i < count; i++)
        {
            Transform baseT = null;
            if (selectedCount > 0)
            {
                if (distributeAcrossSelected)
                {
                    baseT = selected[i % selectedCount];
                }
                else
                {
                    int idx = Random.Range(0, selectedCount);
                    baseT = selected[idx];
                }
            }
            else
            {
                // Fallback to single spawnPoint or cauldron position
                baseT = spawnPoint;
            }

            Vector3 pos; Quaternion rot;
            bool found = false;
            if (baseT != null)
            {
                found = TryGetFreeSpawnPoseNear(baseT, out pos, out rot);
                if (!found)
                {
                    // As a fallback, try anywhere using existing logic
                    found = TryGetFreeSpawnPose(out pos, out rot);
                }
            }
            else
            {
                found = TryGetFreeSpawnPose(out pos, out rot);
            }

            if (!found)
            {
                Debug.Log("Cauldron: No free spawn spot found for cookie in batch (occupied). Skipping one.");
                continue;
            }

            GameObject cookie = Instantiate(cookiePrefab, pos, rot);

            // Optional little pop-up impulse if there is a rigidbody
            if (cookie.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(launchImpulse, ForceMode.Impulse);
            }

            // Track collection for batch gating
            var pickup = cookie.GetComponent<CookiePickup>();
            if (pickup != null)
            {
                _outstandingBatchCookies++;
                pickup.Collected += OnCookieCollected;
            }
        }

        // Start cooldown after the batch
        if (cooldownSeconds > 0f)
            _cooldownUntil = Time.time + cooldownSeconds;

        // Hide prompt after spawning (optional)
        SetPrompt(false);
        _playerInRange = false;
    }

    private void OnCookieCollected(CookiePickup cp)
    {
        // Unsubscribe to avoid leaks
        if (cp != null) cp.Collected -= OnCookieCollected;
        _outstandingBatchCookies = Mathf.Max(0, _outstandingBatchCookies - 1);
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

    // Try to spawn near a specific spawn point with jitter and occupancy checks
    private bool TryGetFreeSpawnPoseNear(Transform baseT, out Vector3 pos, out Quaternion rot)
    {
        Vector3 basePos = baseT != null ? baseT.position : transform.position + spawnOffset;
        Quaternion baseRot = baseT != null ? baseT.rotation : Quaternion.identity;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
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

    // Choose up to 'count' distinct, non-null transforms randomly from the array
    private Transform[] ChooseDistinctSpawnPoints(Transform[] all, int count, out int selectedCount)
    {
        // Gather non-null
        int n = 0;
        for (int i = 0; i < all.Length; i++) if (all[i] != null) n++;
        if (n == 0)
        {
            selectedCount = 0;
            return null;
        }

        // Copy to temp
        Transform[] temp = new Transform[n];
        int idx = 0;
        for (int i = 0; i < all.Length; i++) if (all[i] != null) temp[idx++] = all[i];

        // Fisher-Yates partial shuffle up to k
        int k = Mathf.Clamp(count, 1, temp.Length);
        for (int i = 0; i < k; i++)
        {
            int r = Random.Range(i, temp.Length);
            var t = temp[i]; temp[i] = temp[r]; temp[r] = t;
        }

        // Slice first k
        Transform[] result = new Transform[k];
        for (int i = 0; i < k; i++) result[i] = temp[i];
        selectedCount = k;
        return result;
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
