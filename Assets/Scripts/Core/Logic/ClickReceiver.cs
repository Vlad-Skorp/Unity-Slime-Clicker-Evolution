using UnityEngine;
using UnityEngine.EventSystems;
using SlimeRpgEvolution2D.Logic;
using SlimeRpgEvolution2D.Core;

public class ClickReceiver : MonoBehaviour, IPointerClickHandler
{
    private IDamageable _damageble;
    private Player _player;

    private void Awake()
    {
        _damageble = GetComponent<IDamageable>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_damageble != null && !_damageble.IsDead)  GlobalEvents.SendTargetCliked(_damageble);
    }
}
