using UnityEngine;
using UnityEngine.AI;

public class WitchAI : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    [Header("Patrol Settings")]
    public float wanderRadius = 10f;      // random roam area radius
    public float wanderCooldown = 5f;     // how often to pick new point
    private float wanderTimer;

    [Header("Chase Memory")]
    public float loseSightDelay = 3f;     // seconds before giving up chase
    private float loseSightTimer;

    private const string WITCH_WALK = "isWalking";
    private const string WITCH_RUNNING = "isFoundRunning";

    [SerializeField] private Witch witch;

    private NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    private bool playerVisible;

    public float aggressionLevel = 1f; // starts at 1

    [Header("Combat")]
    [SerializeField] private int touchDamage = 10;
    [SerializeField] private float damageInterval = 1.0f; // seconds between hits while in contact
    [SerializeField] private float hitRadius = 1.2f;       // distance check radius for damage
    private float _nextDamageTime;

    void Awake()
    {
        if (witch == null)
            witch = GetComponent<Witch>();
        if (player == null)
        {
            if (witch != null && witch.player != null)
                player = witch.player.transform;
            else
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderCooldown;
    }

    void Update()
    {
        playerVisible = (witch != null) && witch.CanSeePlayer();

        if (playerVisible && player != null)
        {
            // Reset memory timer if we see the player
            loseSightTimer = loseSightDelay;

            // Run toward player
            agent.speed = runSpeed;
            agent.SetDestination(player.position);
            animator.SetBool(WITCH_RUNNING, true);
            animator.SetBool(WITCH_WALK, false);
        }
        else
        {
            // Still chase if timer > 0 (memory effect)
            if (loseSightTimer > 0 && player != null)
            {
                loseSightTimer -= Time.deltaTime;
                agent.speed = runSpeed;
                agent.SetDestination(player.position); // last known position
                animator.SetBool(WITCH_RUNNING, true);
                animator.SetBool(WITCH_WALK, false);
            }
            else
            {
                // Patrol / Wander
                Wander();
            }
        }

        // Proximity damage (robust even without colliders/triggers)
        ProximityDamage();
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        // if agent reached destination, wait a little before moving again
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool(WITCH_WALK, false); // idle a bit
        }

        if (wanderTimer >= wanderCooldown && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, 3f, -1); // min 3m away
            NavMeshPath path = new NavMeshPath();

            // only set destination if valid path exists
            if (agent.CalculatePath(newPos, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.speed = walkSpeed;
                agent.SetDestination(newPos);
                animator.SetBool(WITCH_WALK, true);
                animator.SetBool(WITCH_RUNNING, false);
                wanderTimer = 0;
            }
        }
    }

    // Improved version with minDistance
    public static Vector3 RandomNavSphere(Vector3 origin, float maxDist, float minDist, int layermask)
    {
        Vector3 randDirection;

        do
        {
            randDirection = Random.insideUnitSphere * maxDist;
        }
        while (randDirection.magnitude < minDist); // keep picking until it's far enough

        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, maxDist, layermask);

        return navHit.position;
    }

    public void IncreaseAggression()
    {
        aggressionLevel += 0.5f; // adjust as needed
        runSpeed += 0.5f;        // make her faster
        walkSpeed += 0.2f;       // also slightly faster walking
    }

    private void ProximityDamage()
    {
        if (player == null) return;
        if (Time.time < _nextDamageTime) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= hitRadius)
        {
            TryDamage(player.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // visualize hit radius
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        if (Time.time < _nextDamageTime) return;
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(touchDamage);
        _nextDamageTime = Time.time + damageInterval;
    }
}
