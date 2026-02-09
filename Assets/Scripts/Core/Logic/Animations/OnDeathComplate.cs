using UnityEngine;

public class OnDeathComplete : StateMachineBehaviour
{

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator == null || animator.gameObject == null) return;

        if (animator.TryGetComponentInParent<Enemy>(out var enemy)) enemy.FinalizeObject();

        else Destroy(animator.gameObject);

    }
}
