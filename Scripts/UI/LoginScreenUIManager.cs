using System;
using Sequence.Demo;
using UnityEngine;

namespace Game.Scripts
{
    public class LoginScreenUIManager : MonoBehaviour
    {
        [SerializeField] private ButtonSwitcher _leftButtons;
        [SerializeField] private ButtonSwitcher _rightButtons;
        
        private LoginPanel _loginPanel;
        private PlayFabLoginPanel _playFabLoginPanel;
        private GuestLoginPanel _guestLoginPanel;

        private void Awake()
        {
            _loginPanel = GetComponentInChildren<LoginPanel>();
            _playFabLoginPanel = GetComponentInChildren<PlayFabLoginPanel>();
            _guestLoginPanel = GetComponentInChildren<GuestLoginPanel>();
        }

        private void Start()
        {
            if (_loginPanel == null)
            {
                Debug.LogError("LoginPanel not found!");
            }

            _loginPanel.Open();
        }
        
        public void SwitchToPlayFabLoginPanel()
        {
            _loginPanel.Close();
            _playFabLoginPanel.Open();
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
    }
}