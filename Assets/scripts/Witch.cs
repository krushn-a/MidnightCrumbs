using System;
using UnityEngine;
using UnityEngine.AI;

public class Witch : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRange = 60f;
    public float visionAngle = 60f;
    public float visionRadius = 0.5f;   // thickness of SphereCast

    [SerializeField] private LayerMask obstacleMask;      // walls, trees, gravestones
    [SerializeField] private LayerMask playerMask;        // player layer
    public GameObject player;

    public bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // ✅ Step 1: Check if player within range
        if (distanceToPlayer <= visionRange)
        {
            // ✅ Step 2: Check if inside field of view
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle <= visionAngle / 2f)
            {
                // ✅ Step 3: SphereCast to detect the player in vision cone
                RaycastHit sphereHit;
                if (Physics.SphereCast(transform.position + Vector3.up, visionRadius, directionToPlayer, out sphereHit, visionRange, playerMask))
                {
                    // ✅ Step 4: Now use Raycast to check if line of sight is blocked
                    RaycastHit rayHit;
                    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out rayHit, visionRange, obstacleMask | playerMask))
                    {
                        //Debug.Log("Raycast hit: " + rayHit.transform.tag);
                        if (rayHit.transform.CompareTag("Player"))
                        {
                            //Debug.Log("👀 Player Spotted!");
                            return true; // Player visible
                        }
                    }
                }
            }
        }

        //Debug.Log("❌ Player NOT spotted");
        return false;
    }

    private void Update()
    {
        CanSeePlayer();
    }

    public bool IsWalking() 
    {
        return !CanSeePlayer();
    }

    public bool IsRunning()
    {
        return CanSeePlayer();
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, visionRange);

    //    // Draw forward direction
    //    Vector3 forward = transform.forward * visionRange;
    //    Vector3 leftLimit = Quaternion.Euler(0, -visionAngle / 2f, 0) * forward;
    //    Vector3 rightLimit = Quaternion.Euler(0, visionAngle / 2f, 0) * forward;

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawRay(transform.position, leftLimit);
    //    Gizmos.DrawRay(transform.position, rightLimit);

    //    // If player spotted, draw green ray
    //    if (CanSeePlayer() && player != null) // Fixed the method call
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(transform.position + Vector3.up, player.transform.position);
    //    }
    //}
}
