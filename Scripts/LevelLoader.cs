using System;
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
        }
    }
}