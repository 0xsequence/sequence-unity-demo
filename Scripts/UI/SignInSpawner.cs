using System;
using System.Collections.Generic;
using Sequence.Authentication;
using UnityEngine;

namespace Game.Scripts
{
    public class SignInSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _googleSignInButton;
        [SerializeField] private GameObject _facebookSignInButton;
        [SerializeField] private GameObject _discordSignInButton;
        [SerializeField] private GameObject _appleSignInButton;
        [SerializeField] private GameObject _playFabSignInButton;
        [SerializeField] private GameObject _guestSignInButton;
        [SerializeField] private GameObject _allUsedMessage;

        private void Awake()
        {
            List<GameObject> toDisable = new List<GameObject>();
#if UNITY_IOS && !UNITY_EDITOR
            toDisable.Add(_facebookSignInButton);
            toDisable.Add(_discordSignInButton);
#elif UNITY_ANDROID && !UNITY_EDITOR
            toDisable.Add(_facebookSignInButton);
            toDisable.Add(_discordSignInButton);
            toDisable.Add(_appleSignInButton);
#else
            toDisable.Add(_facebookSignInButton);
            toDisable.Add(_discordSignInButton);
            toDisable.Add(_appleSignInButton);
#endif
            int items = toDisable.Count;
            for (int i = 0; i < items; i++)
            {
                toDisable[i].SetActive(false);
            }
            
            ActivateAllUsedMessage();
        }
        
        public void DisableMethod(LoginMethod method)
        {
            switch (method)
            {
                case LoginMethod.Google:
                    _googleSignInButton.SetActive(false);
                    break;
                case LoginMethod.Facebook:
                    _facebookSignInButton.SetActive(false);
                    break;
                case LoginMethod.Discord:
                    _discordSignInButton.SetActive(false);
                    break;
                case LoginMethod.Apple:
                    _appleSignInButton.SetActive(false);
                    break;
                case LoginMethod.PlayFab:
                    _playFabSignInButton.SetActive(false);
                    break;
                case LoginMethod.Guest:
                    _guestSignInButton.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            ActivateAllUsedMessage();
        }

        private bool AllDisabled()
        {
            return !_googleSignInButton.activeSelf && !_facebookSignInButton.activeSelf && !_discordSignInButton.activeSelf && !_appleSignInButton.activeSelf && !_playFabSignInButton.activeSelf && !_guestSignInButton.activeSelf;
        }

        private void ActivateAllUsedMessage()
        {
            if (AllDisabled())
            {
                if (SequenceConnector.Instance.Wallet == null)
                {
                    Debug.LogError("Wallet is null. There should be a login method available.");
                }
                _allUsedMessage.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                _allUsedMessage.SetActive(false);
            }
        }
    }
}