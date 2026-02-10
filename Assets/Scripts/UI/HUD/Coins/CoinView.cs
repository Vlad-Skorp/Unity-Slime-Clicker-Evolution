using TMPro;
using UnityEngine;


namespace SlimeRpgEvolution2D.UI.HUD
{
    public class CoinView : MonoBehaviour
    {
        [SerializeReference] private TextMeshProUGUI _text;

        public void SetCoin(int value)
        {
            _text.text = value.ToString();
        }
    }
}
