using UnityEngine;
using TMPro;
using UnityEngine.AI;
using StarterAssets;

// Attach this to a UI GameObject (e.g., a Canvas child) and assign a TMP_Text.
// It periodically finds the nearest CookiePickup and shows its distance.
public class CookieProximityUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text distanceText;        // Text element to display distance
    public Transform target;             // Usually the player. Auto-assigned if left null.
    public FirstPersonController fpcRef; // If present, use this transform for position

    [Header("Display")]
    [Tooltip("Text format. {0} = distance in meters.")]
    public string format = "Nearest cookie: {0:0.0} m";
    public bool showWhenNone = true;
    public string noneText = "No cookies nearby";

    [Header("Update")]
    [Tooltip("How often to refresh the distance (seconds). Use small values for responsiveness.")]
    public float updateInterval = 0.25f;
    private float _nextUpdateAt;

    [Header("Targeting")]
    [Tooltip("If true, compute distance on the XZ plane (ignoring height). If false, use full 3D distance.")]
    public bool useHorizontalDistance = true;
    [Tooltip("How much closer (in meters) a different cookie must be to switch the target. Prevents jitter.")]
    public float retargetHysteresis = 0.5f;

    private CookiePickup _currentTarget;

    [Header("Gizmos")]
    [Tooltip("Draw a gizmo path from the player to the current target cookie in the Scene view.")]
    public bool drawGizmos = true;
    [Tooltip("If true, only draw gizmos when this GameObject is selected.")]
    public bool onlyWhenSelected = true;
    public Color gizmoColor = new Color(1f, 0.64f, 0f, 0.9f); // orange-ish
    [Tooltip("Radius of small spheres drawn along the path.")]
    public float gizmoSphereRadius = 0.08f;
    [Tooltip("Use the NavMesh for the path instead of a straight line.")]
    public bool useNavMeshPath = true;
    [Tooltip("Radius used to sample onto the NavMesh near endpoints.")]
    public float navMeshSampleRadius = 1.0f;
    [Tooltip("Optional reference to a NavMeshAgent to derive the agent type for path calculation.")]
    public NavMeshAgent agentRef;

    private void Reset()
    {
        if (distanceText == null)
            distanceText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        if (target == null)
        {
            FirstPersonController fpc = null;
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            fpc = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
#else
            fpc = FindObjectOfType<FirstPersonController>();
#endif
            if (fpc != null)
            {
                fpcRef = fpc;
                target = fpc.transform;
            }
            else if (Camera.main != null) target = Camera.main.transform;
        }

        if (agentRef == null && target != null)
        {
            agentRef = target.GetComponentInParent<NavMeshAgent>();
        }
    }

    private void OnEnable()
    {
        _nextUpdateAt = 0f;
        UpdateNow();
    }

    private void Update()
    {
        if (Time.unscaledTime >= _nextUpdateAt)
        {
            UpdateNow();
            _nextUpdateAt = Time.unscaledTime + Mathf.Max(0.05f, updateInterval);
        }
    }

    private void UpdateNow()
    {
        if (distanceText == null) return;

        // Find the best candidate right now
        float candidateDist;
        Vector3 playerPos = GetPlayerPosition();
        var candidate = FindNearestCookie(playerPos, out candidateDist);

        // If we have no current or current is invalid, adopt candidate directly
        if (_currentTarget == null || !_currentTarget || !_currentTarget.isActiveAndEnabled)
        {
            _currentTarget = candidate;
        }
        else if (candidate != null && candidate != _currentTarget)
        {
            // Compare with hysteresis to avoid flicker
            float currentDist = ComputeDistance(playerPos, _currentTarget.transform.position);
            if (candidateDist + Mathf.Epsilon < currentDist - Mathf.Max(0f, retargetHysteresis))
            {
                _currentTarget = candidate;
            }
        }

        if (_currentTarget != null && _currentTarget.isActiveAndEnabled)
        {
            float d = ComputeDistance(playerPos, _currentTarget.transform.position);
            distanceText.text = string.Format(format, d);
            if (!distanceText.gameObject.activeSelf) distanceText.gameObject.SetActive(true);
        }
        else
        {
            if (showWhenNone)
            {
                distanceText.text = noneText;
                if (!distanceText.gameObject.activeSelf) distanceText.gameObject.SetActive(true);
            }
            else
            {
                if (distanceText.gameObject.activeSelf) distanceText.gameObject.SetActive(false);
            }
        }
    }

    private CookiePickup FindNearestCookie(Vector3 pos, out float distance)
    {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        var cookies = UnityEngine.Object.FindObjectsByType<CookiePickup>(FindObjectsSortMode.None);
#else
        var cookies = FindObjectsOfType<CookiePickup>();
#endif

        CookiePickup best = null;
        float bestSq = float.PositiveInfinity;
        for (int i = 0; i < cookies.Length; i++)
        {
            var c = cookies[i];
            if (c == null || !c.isActiveAndEnabled) continue;

            float sq;
            if (useHorizontalDistance)
            {
                Vector3 a = c.transform.position; a.y = 0f;
                Vector3 b = pos; b.y = 0f;
                sq = (a - b).sqrMagnitude;
            }
            else
            {
                sq = (c.transform.position - pos).sqrMagnitude;
            }
            if (sq < bestSq)
            {
                bestSq = sq;
                best = c;
            }
        }

        if (best != null)
        {
            distance = Mathf.Sqrt(bestSq);
            return best;
        }

        distance = 0f;
        return null;
    }

    private float ComputeDistance(Vector3 a, Vector3 b)
    {
        if (useHorizontalDistance)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || onlyWhenSelected) return;
        DrawPathGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !onlyWhenSelected) return;
        DrawPathGizmos();
    }

    private void DrawPathGizmos()
    {
        if (target == null || _currentTarget == null || !_currentTarget) return;

        Vector3 start = GetPlayerPosition();
        Vector3 end = _currentTarget.transform.position;

        Gizmos.color = gizmoColor;

        if (useHorizontalDistance)
        {
            start.y = end.y; // flatten to same height for a cleaner look
        }
        if (useNavMeshPath)
        {
            // Try to compute a NavMesh path between start and end
            NavMeshHit sHit;
            NavMeshHit eHit;
            if (NavMesh.SamplePosition(start, out sHit, navMeshSampleRadius, NavMesh.AllAreas) &&
                NavMesh.SamplePosition(end, out eHit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                // Build a filter with a proper agent type id
                NavMeshQueryFilter filter = new NavMeshQueryFilter
                {
                    agentTypeID = GetAgentTypeId(),
                    areaMask = NavMesh.AllAreas
                };
#if UNITY_2022_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                if (NavMesh.CalculatePath(sHit.position, eHit.position, filter, path))
#else
                if (NavMesh.CalculatePath(sHit.position, eHit.position, NavMesh.AllAreas, path))
#endif
                {
                    Vector3[] corners = path.corners;
                    if (corners != null && corners.Length > 1)
                    {
                        for (int i = 0; i < corners.Length - 1; i++)
                        {
                            Vector3 a = corners[i];
                            Vector3 b = corners[i + 1];
                            if (useHorizontalDistance) { a.y = end.y; b.y = end.y; }
                            Gizmos.DrawLine(a, b);
                            Gizmos.DrawSphere(a, gizmoSphereRadius);
                        }
                        Gizmos.DrawSphere(corners[corners.Length - 1], gizmoSphereRadius);
                        return;
                    }
                }
            }
        }

        // Fallback: straight line
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(start, gizmoSphereRadius);
        Gizmos.DrawSphere(end, gizmoSphereRadius);
    }

    private Vector3 GetPlayerPosition()
    {
        if (fpcRef != null) return fpcRef.transform.position;
        if (target != null) return target.position;
        if (Camera.main != null) return Camera.main.transform.position;
        return Vector3.zero;
    }

    private int GetAgentTypeId()
    {
        if (agentRef != null) return agentRef.agentTypeID;

        // Try to infer from any existing NavMesh settings
        int settingsCount = NavMesh.GetSettingsCount();
        if (settingsCount > 0)
        {
            var settings = NavMesh.GetSettingsByIndex(0);
            return settings.agentTypeID;
        }

        // Fallback default
        return 0;
    }
}
