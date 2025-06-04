using System;
using System.Collections.Generic;
using Sequence.Authentication;
using Sequence.Demo;
using Sequence.EmbeddedWallet;
using UnityEngine;

namespace Game.Scripts
{
    public class EmailLoginPanel : UIPanel
    {
        private MultifactorAuthenticationPage _mfaPage;
        private EmailLoginPage _emailLoginPage;
        private ILogin _login;
        private LoginPanel _loginPanel;

        protected override void Awake()
        {
            base.Awake();
            _mfaPage = GetComponentInChildren<MultifactorAuthenticationPage>();
            _emailLoginPage = GetComponentInChildren<EmailLoginPage>();

            _login = SequenceLogin.GetInstance();
            _login.OnMFAEmailSent += OnMFAEmailSentHandler;
            
            _emailLoginPage.SetupLogin(_login);
            _mfaPage.SetupLogin(_login);

            _login.OnLoginFailed += OnLoginFailed;

            _loginPanel = FindObjectOfType<LoginPanel>();
        }

        public override void Open(params object[] args)
        {
            base.Open(args);
            _loginPanel.Close();
        }

        private void OnLoginFailed(string error, LoginMethod method, string email, List<LoginMethod> methods)
        {
            Close();
        }

        public override void Close()
        {
            base.Close();
            _loginPanel.Open();
        }

        private void OnDestroy()
        {
            _login.OnMFAEmailSent -= OnMFAEmailSentHandler;
            _login.OnLoginFailed -= OnLoginFailed;
        }

        private void OnMFAEmailSentHandler(string email)
        {
            Debug.Log($"Successfully sent MFA email to {email}");
            StartCoroutine(SetUIPage(_mfaPage, email));
        }
    }
}