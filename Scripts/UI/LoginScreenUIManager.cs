using System;
using Sequence.Authentication;
using Sequence.Demo;
using Sequence.EmbeddedWallet;
using UnityEngine;

namespace Game.Scripts
{
    public class LoginScreenUIManager : MonoBehaviour
    {
        [SerializeField] private ButtonSwitcher _leftButtons;
        [SerializeField] private ButtonSwitcher _rightButtons;
        [SerializeField] private GameObject _cancelAndReturnToGameButton;
        
        private LoginPanel _loginPanel;
        private PlayFabLoginPanel _playFabLoginPanel;
        private GuestLoginPanel _guestLoginPanel;
        private bool _federateAuthMode = false;
        private SignInSpawner _signInSpawner;

        private void Awake()
        {
            _loginPanel = GetComponentInChildren<LoginPanel>();
            _playFabLoginPanel = GetComponentInChildren<PlayFabLoginPanel>();
            _guestLoginPanel = GetComponentInChildren<GuestLoginPanel>();
            _signInSpawner = GetComponentInChildren<SignInSpawner>();
        }

        private void Start()
        {
            if (_loginPanel == null)
            {
                Debug.LogError("LoginPanel not found!");
            }

            _loginPanel.Open();
        }

        public void PlayFabLogin()
        {
            _playFabLoginPanel.Open();
        }
        
        public void SwitchToPlayFabLoginPanel()
        {
            _loginPanel.Close();
            _playFabLoginPanel.Open(_federateAuthMode);
            _leftButtons.Switch();
        }
        
        public void SwitchToLoginPanelFromPlayFab()
        {
            _playFabLoginPanel.Close();
            _loginPanel.Open();
            _leftButtons.Switch();
        }

        public void SwitchToGuestLoginPanel()
        {
            _loginPanel.Close();
            _guestLoginPanel.Open();
            _rightButtons.Switch();
        }
        
        public void SwitchToLoginPanelFromGuest()
        {
            _guestLoginPanel.Close();
            _loginPanel.Open();
            _rightButtons.Switch();
        }

        public void FederateAuth()
        {
            DisableGuestLogin();
            _cancelAndReturnToGameButton.SetActive(true);
            IWallet wallet = SequenceConnector.Instance.Wallet;
            wallet.OnAccountListGenerated += OnAccountListGenerated;
            wallet.GetAccountList();
        }

        private void DisableGuestLogin()
        {
            _signInSpawner.DisableMethod(LoginMethod.Guest);
        }
        
        private void OnAccountListGenerated(IntentResponseAccountList accounts)
        {
            SequenceConnector.Instance.Wallet.OnAccountListGenerated -= OnAccountListGenerated;
            
            Account[] accountList = accounts.accounts;
            int length = accountList.Length;
            for (int i = 0; i < length; i++)
            {
                Account account = accountList[i];
                if (account.identityType == IdentityType.Guest || account.identityType == IdentityType.Email)
                {
                    continue;
                }
                else if (account.identityType == IdentityType.PlayFab)
                {
                    _signInSpawner.DisableMethod(LoginMethod.PlayFab);
                }
                else
                {
                    if (account.issuer.Contains("google"))
                    {
                        _signInSpawner.DisableMethod(LoginMethod.Google);
                    }
                    else if (account.issuer.Contains("facebook"))
                    {
                        _signInSpawner.DisableMethod(LoginMethod.Facebook);
                    }
                    else if (account.issuer.Contains("discord"))
                    {
                        _signInSpawner.DisableMethod(LoginMethod.Discord);
                    }
                    else if (account.issuer.Contains("apple"))
                    {
                        _signInSpawner.DisableMethod(LoginMethod.Apple);
                    }
                }
            }
        }
    }
}