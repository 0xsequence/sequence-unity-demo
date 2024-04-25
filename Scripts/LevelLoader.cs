using System;
using Sequence.WaaS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class LevelLoader : MonoBehaviour
    {
        private void Awake()
        {
            WaaSWallet.OnWaaSWalletCreated += OnWaaSWalletCreated;
        }
        
        private void OnWaaSWalletCreated(WaaSWallet wallet)
        {
            LoadGame();
        }

        private void OnDestroy()
        {
            WaaSWallet.OnWaaSWalletCreated -= OnWaaSWalletCreated;
        }

        public void LoadGame()
        {
            SceneManager.LoadScene("MenuScene");
        }
    }
}