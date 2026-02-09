using UnityEngine;

public class OnRespawnComplete : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetComponentInParent<Enemy>() != null)
        {
            animator.GetComponentInParent<Enemy>().SetCombatReady(true);
        }
    }
}
