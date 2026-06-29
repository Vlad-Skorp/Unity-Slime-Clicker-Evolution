using UnityEngine;

public class OnRespawnComplete : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1. Старый рабочий код готовности к бою
        if (animator.GetComponentInParent<Enemy>() != null)
        {
            animator.GetComponentInParent<Enemy>().SetCombatReady(true);
        }

        // 2. НОВЫЙ КОД: Ищем компонент интерфейса на заспавненном слизне
        UIEnemyInfo uiInfo = animator.GetComponentInChildren<UIEnemyInfo>();
        if (uiInfo == null && animator.transform.parent != null)
        {
            uiInfo = animator.transform.parent.GetComponentInChildren<UIEnemyInfo>();
        }

        // 3. Вызываем расчет позиции ОДИН РАЗ, когда слизень гарантированно принял свой полный размер
        if (uiInfo != null)
        {
            uiInfo.UpdateUIPosition();
        }
    }
}
 