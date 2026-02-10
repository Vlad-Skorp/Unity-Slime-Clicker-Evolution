using UnityEngine;



namespace SlimeRpgEvolution2D.UI.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Layers")]
        [SerializeField] private GameObject _hudLayer;
        [SerializeField] private GameObject _popupLayer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return; 
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitLayers();
        }

        private void InitLayers()
        {
            if (_hudLayer != null) _hudLayer.SetActive(true);
            if (_popupLayer != null) _popupLayer.SetActive(false);
        }

        public void OpenPopup(GameObject prefab)
        {
            if (prefab == null || _popupLayer == null) return;

            _popupLayer.SetActive(true);

            GameObject popup = Instantiate(prefab, _popupLayer.transform);
        }

        public void CloseAllPopups()
        {
            foreach (Transform child in _popupLayer.transform)
            {
                Destroy(child.gameObject);
            }
            _popupLayer.SetActive(false);
        }

        public void ToggleHUD(bool isVisible)
        {
            if (_hudLayer != null) _hudLayer.SetActive(isVisible);
        }
      
    }
}
