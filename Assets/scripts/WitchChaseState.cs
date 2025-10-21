//using UnityEngine;

//public class WitchChaseState : WitchState
//{
//    private float loseSightTimer;

//    public WitchChaseState(WitchAI witchAI) : base(witchAI) { }

//    public override void Enter()
//    {
//        witchAI.animator.SetBool("isFoundRunning", true);
//        witchAI.animator.SetBool("isWalking", false);
//        loseSightTimer = witchAI.loseSightDelay;
//    }

//    public override void Update()
//    {
//        if (witchAI.player == null) return;

//        if (witchAI.Witch.CanSeePlayer())
//        {
//            loseSightTimer = witchAI.loseSightDelay;
//            witchAI.agent.speed = witchAI.runSpeed;
//            witchAI.agent.SetDestination(witchAI.player.position);
//        }
//        else
//        {
//            loseSightTimer -= Time.deltaTime;
//            if (loseSightTimer > 0)
//            {
//                witchAI.agent.SetDestination(witchAI.player.position);
//            }
//            else
//            {
//                witchAI.ChangeState(new WitchPatrolState(witchAI));
//            }
//        }

//        witchAI.ProximityDamage();
//    }
//}
