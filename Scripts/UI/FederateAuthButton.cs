using System.Threading.Tasks;
using Sequence.EmbeddedWallet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts
{
    public class FederateAuthButton : MonoBehaviour
    {
        public void FederateAuth()
        {
            SequenceLogin.GetInstanceToFederateAuth(SequenceConnector.Instance.Wallet.GetWalletAddress());
            SetupAuthFederation();
        }

        private async Task SetupAuthFederation()
        {
            await Task.Delay(100);
            SceneManager.LoadScene("LoginScene");
            LoginScreenUIManager loginScreenUIManager = FindObjectOfType<LoginScreenUIManager>();
            while (loginScreenUIManager == null)
            {
                await Task.Yield();
                loginScreenUIManager = FindObjectOfType<LoginScreenUIManager>();
            }
            loginScreenUIManager.FederateAuth();
        }
    }
}