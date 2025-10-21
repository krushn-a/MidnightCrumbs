using UnityEngine;
using UnityEngine.AI;

public class WitchAI : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    [Header("Patrol Settings")]
    public float wanderRadius = 10f;
    public float wanderCooldown = 5f;
    private float wanderTimer;

    [Header("Chase Memory")]
    public float loseSightDelay = 3f;
    private float loseSightTimer;

    [Header("Audio")]
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private float walkingSoundVolume = 0.5f;
    [SerializeField] private float runningSoundVolume = 0.7f;
    private AudioSource audioSource;
    private bool wasPlayingBeforePause = false;

    private const string WITCH_WALK = "isWalking";
    private const string WITCH_RUNNING = "isFoundRunning";

    [SerializeField] private Witch witch;

    private NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    private bool playerVisible;

    public float aggressionLevel = 1f;

    [Header("Combat")]
    [SerializeField] private int touchDamage = 10;
    [SerializeField] private float damageInterval = 1.0f;
    [SerializeField] private float hitRadius = 1.2f;
    private float _nextDamageTime;

    private enum WitchState { Idle, Walking, Running }
    private WitchState currentState = WitchState.Idle;

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

        EnsureAudioSource();
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderCooldown;
    }

    void Update()
    {
        HandlePauseState();

        playerVisible = (witch != null) && witch.CanSeePlayer();

        if (playerVisible && player != null)
        {
            loseSightTimer = loseSightDelay;

            agent.speed = runSpeed;
            agent.SetDestination(player.position);
            animator.SetBool(WITCH_RUNNING, true);
            animator.SetBool(WITCH_WALK, false);

            SetState(WitchState.Running);
        }
        else
        {
            if (loseSightTimer > 0 && player != null)
            {
                loseSightTimer -= Time.deltaTime;
                agent.speed = runSpeed;
                agent.SetDestination(player.position);
                animator.SetBool(WITCH_RUNNING, true);
                animator.SetBool(WITCH_WALK, false);

                SetState(WitchState.Running);
            }
            else
            {
                Wander();
            }
        }

        ProximityDamage();
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool(WITCH_WALK, false);
            SetState(WitchState.Idle);
        }

        if (wanderTimer >= wanderCooldown && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, 3f, -1);
            NavMeshPath path = new NavMeshPath();

            if (agent.CalculatePath(newPos, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.speed = walkSpeed;
                agent.SetDestination(newPos);
                animator.SetBool(WITCH_WALK, true);
                animator.SetBool(WITCH_RUNNING, false);
                wanderTimer = 0;

                SetState(WitchState.Walking);
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float maxDist, float minDist, int layermask)
    {
        Vector3 randDirection;

        do
        {
            randDirection = Random.insideUnitSphere * maxDist;
        }
        while (randDirection.magnitude < minDist);

        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, maxDist, layermask);

        return navHit.position;
    }

    public void IncreaseAggression()
    {
        aggressionLevel += 0.5f;
        runSpeed += 0.5f;
        walkSpeed += 0.2f;
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

    private void EnsureAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 30f;
    }

    private void HandlePauseState()
    {
        if (audioSource == null) return;

        if (Time.timeScale == 0f)
        {
            if (audioSource.isPlaying)
            {
                wasPlayingBeforePause = true;
                audioSource.Pause();
            }
        }
        else
        {
            if (wasPlayingBeforePause && !audioSource.isPlaying)
            {
                audioSource.UnPause();
                wasPlayingBeforePause = false;
            }
        }
    }

    private void SetState(WitchState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case WitchState.Idle:
                StopFootstepSounds();
                break;

            case WitchState.Walking:
                PlayWalkingSound();
                break;

            case WitchState.Running:
                PlayRunningSound();
                break;
        }
    }

    private void PlayWalkingSound()
    {
        if (audioSource == null || walkingSound == null) return;
        if (Time.timeScale == 0f) return;

        if (audioSource.clip != walkingSound)
        {
            audioSource.clip = walkingSound;
            audioSource.volume = walkingSoundVolume;
            audioSource.Play();
        }
    }

    private void PlayRunningSound()
    {
        if (audioSource == null || runningSound == null) return;
        if (Time.timeScale == 0f) return;

        if (audioSource.clip != runningSound)
        {
            audioSource.clip = runningSound;
            audioSource.volume = runningSoundVolume;
            audioSource.Play();
        }
    }

    private void StopFootstepSounds()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            wasPlayingBeforePause = false;
        }
    }

    private void OnDisable()
    {
        StopFootstepSounds();
    }
}
