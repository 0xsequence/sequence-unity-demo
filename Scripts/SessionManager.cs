using System.Collections.Generic;
using Sequence.Config;
using Sequence.Utils.SecureStorage;
using Sequence.WaaS;
using UnityEngine;

namespace Game.Scripts
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance;
        
        private List<WaaSWallet> _sessions = new List<WaaSWallet>();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            WaaSWallet.OnWaaSWalletCreated += AddSession;
        }

        private void AddSession(WaaSWallet sessionWallet)
        {
            _sessions.Add(sessionWallet);
        }

        private void OnApplicationQuit()
        {
            SequenceConnector.Instance.SubmitQueuedTransactions(true);

            if (SequenceConfig.GetConfig().StoreSessionPrivateKeyInSecureStorage && SecureStorageFactory.IsSupportedPlatform())
            {
                return;
            }
            
            int sessionsCount = _sessions.Count;
            for (int i = 0; i < sessionsCount; i++)
            {
                _sessions[i].DropThisSession();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (SequenceConnector.Instance == null || SequenceConnector.Instance.Inventory == null)
            {
                return;
            }
            
            if (!hasFocus) // app is in background
            {
                SequenceConnector.Instance.SubmitQueuedTransactions(true);
            }
            else
            {
                SequenceConnector.Instance.Inventory.RefreshTokenBalances();
            }
        }

        private void OnDestroy()
        {
            WaaSWallet.OnWaaSWalletCreated -= AddSession;
            _sessions = new List<WaaSWallet>();
        }
    }
}