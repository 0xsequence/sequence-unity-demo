using System;
using Game.Scripts.UI;
using MoreMountains.TopDownEngine;
using Sequence.Authentication;
using Sequence.Demo;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class MainMenuSceneUIManager : UIPanel
    {
        [SerializeField] private TextMeshProUGUI _loggedInAsText;
        
        private MainMenuPage _mainMenuPage;
        private ShopPanel _shopPanel;
        private AccountPanel _accountPanel;
        
        protected override void Awake()
        {
            base.Awake();
            _mainMenuPage = GetComponentInChildren<MainMenuPage>();
            _shopPanel = GetComponentInChildren<ShopPanel>();
            _accountPanel = GetComponentInChildren<AccountPanel>();
        }

        private void Start()
        {
            Open();

            string email = PlayerPrefs.GetString(OpenIdAuthenticator.LoginEmail);
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "Guest";
            }
            _loggedInAsText.text = email.ToUpper();
        }

        public void OpenMainMenuPage()
        {
            StartCoroutine(SetUIPage(_mainMenuPage));
        }
        
        public void OpenShopPanel()
        {
            _shopPanel.Open();
        }
        
        public void OpenAccountPanel()
        {
            _accountPanel.Open(_loggedInAsText.text);
        }
    }
}