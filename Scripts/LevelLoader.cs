using System;
using System.Collections;
using Game.Scripts.UI;
using Sequence.EmbeddedWallet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class LevelLoader : MonoBehaviour
    {
        private void Awake()
        {
            SequenceWallet.OnWalletCreated += OnWalletCreated;
            SequenceWallet.OnAccountFederated += OnAccountFederated;
        }
        
        private void OnWalletCreated(SequenceWallet wallet)
        {
            LoadGame();
        }

        private void OnDestroy()
        {
            SequenceWallet.OnWalletCreated -= OnWalletCreated;
        }

        public void LoadGame()
        {
            SceneManager.LoadScene("MenuScene");
            StartCoroutine(ShowSignedInPopup());
        }

        private IEnumerator ShowSignedInPopup()
        {
            SignedInSuccessfullyPopup signedInSuccessfullyPopup = FindObjectOfType<SignedInSuccessfullyPopup>();
            while (signedInSuccessfullyPopup == null)
            {
                yield return null;
                signedInSuccessfullyPopup = FindObjectOfType<SignedInSuccessfullyPopup>();
            }
            signedInSuccessfullyPopup.Open();
        }
        
        private void OnAccountFederated(Account account)
        {
            LoadGame();
        }
    }
}