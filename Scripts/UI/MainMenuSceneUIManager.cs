using System;
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
        private ShopUpgradesMenuPage _shopUpgradesMenuPage;
        private ShopHatsMenuPage _shopHatsMenuPage;
        private SessionTransactionsPage _sessionTransactionsPage;
        
        protected override void Awake()
        {
            base.Awake();
            _mainMenuPage = GetComponentInChildren<MainMenuPage>();
            _shopUpgradesMenuPage = GetComponentInChildren<ShopUpgradesMenuPage>();
            _shopHatsMenuPage = GetComponentInChildren<ShopHatsMenuPage>();
            _sessionTransactionsPage = GetComponentInChildren<SessionTransactionsPage>();
        }

        private void Start()
        {
            Open();

            string email = PlayerPrefs.GetString(OpenIdAuthenticator.LoginEmail);
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "Guest";
            }
            _loggedInAsText.text = "Logged in as: " + email;
        }

        public void OpenMainMenuPage()
        {
            StartCoroutine(SetUIPage(_mainMenuPage));
        }
        
        public void OpenShopUpgradesMenuPage()
        {
            StartCoroutine(SetUIPage(_shopUpgradesMenuPage));
        }
        
        public void OpenShopHatsMenuPage()
        {
            StartCoroutine(SetUIPage(_shopHatsMenuPage));
        }
        
        public void OpenSessionTransactionsPage()
        {
            StartCoroutine(SetUIPage(_sessionTransactionsPage));
        }
    }
}