using UnityEngine;

public class WitchAnimator : MonoBehaviour
{
    private const string WITCH_WALK = "isWalking";
    private const string WITCH_RUNNING = "isFoundRunning";

    private Animator witchAnimator;

    [SerializeField] private Witch witch;

    private void Awake() 
    {
        witchAnimator = GetComponent<Animator>();
    }
    private void Update()
    {
        witchAnimator.SetBool(WITCH_WALK, witch.IsWalking());
        witchAnimator.SetBool(WITCH_RUNNING, witch.IsRunning());
    }
}
