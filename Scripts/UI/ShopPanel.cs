using Sequence.Demo;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ShopPanel : UIPanel
    {
        [SerializeField] private Sprite _upgradesToggle;
        [SerializeField] private Sprite _hatsToggle;
        [SerializeField] private Image _toggleImage;
        
        private ShopHatsMenuPage _shopHatsMenuPage;
        private ShopUpgradesMenuPage _shopUpgradesMenuPage;
        
        enum ShopTab
        {
            Upgrades,
            Hats
        }
        
        private ShopTab _currentTab = ShopTab.Upgrades;
        
        protected override void Awake()
        {
            base.Awake();
            _shopHatsMenuPage = GetComponentInChildren<ShopHatsMenuPage>();
            _shopUpgradesMenuPage = GetComponentInChildren<ShopUpgradesMenuPage>();
        }

        public override void Open(params object[] args)
        {
            base.Open(args);
            _toggleImage.sprite = _upgradesToggle;
            _currentTab = ShopTab.Upgrades;
        }

        public void ToggleShopTab()
        {
            if (_currentTab == ShopTab.Upgrades)
            {
                _currentTab = ShopTab.Hats;
                _toggleImage.sprite = _hatsToggle;
                StartCoroutine(SetUIPage(_shopHatsMenuPage));
            }
            else
            {
                _currentTab = ShopTab.Upgrades;
                _toggleImage.sprite = _upgradesToggle;
                StartCoroutine(SetUIPage(_shopUpgradesMenuPage));
            }
        }
        
        public void DebugAddTokens()
        {
            if (SequenceConnector.Instance == null)
            {
                Debug.LogError("User is not logged in. Cannot add tokens.");
            }
            
            SequenceConnector.Instance.DebugAddTokens();
        }
    }
}