//using UnityEngine;
//using UnityEngine.AI;

//public class WitchPatrolState : WitchState
//{
//    private float wanderTimer;

//    public WitchPatrolState(WitchAI witchAI) : base(witchAI) { }

//    public override void Enter()
//    {
//        witchAI.animator.SetBool("isWalking", true);
//        witchAI.animator.SetBool("isFoundRunning", false);
//        wanderTimer = 0f;
//    }

//    public override void Update()
//    {
//        wanderTimer += Time.deltaTime;

//        if (witchAI.Witch.CanSeePlayer())
//        {
//            witchAI.ChangeState(new WitchChaseState(witchAI));
//            return;
//        }

//        if (!witchAI.agent.pathPending && witchAI.agent.remainingDistance <= witchAI.agent.stoppingDistance)
//        {
//            if (wanderTimer >= witchAI.wanderCooldown)
//            {
//                Vector3 newPos = WitchAI.RandomNavSphere(witchAI.transform.position, witchAI.wanderRadius, 3f, -1);
//                NavMeshPath path = new NavMeshPath();

//                if (witchAI.agent.CalculatePath(newPos, path) && path.status == NavMeshPathStatus.PathComplete)
//                {
//                    witchAI.agent.speed = witchAI.walkSpeed;
//                    witchAI.agent.SetDestination(newPos);
//                    wanderTimer = 0;
//                }
//            }
//        }
//    }
//}
