using TMPro;
using UnityEngine;


namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CounterView : MonoBehaviour
    {
        [SerializeReference] private TextMeshProUGUI _text;

        public void SetValue(int value)
        {
            _text.text = value.ToString();
        }
    }
}
