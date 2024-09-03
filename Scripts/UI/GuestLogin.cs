using Sequence.Demo;
using Sequence.EmbeddedWallet;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class GuestLogin : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _errorText;
        
        private SequenceLogin _sequenceLogin;
        
        public void Login()
        {
            _sequenceLogin = SequenceLogin.GetInstance();
            _sequenceLogin.OnLoginFailed += (error, method, email, methods) =>
            {
                _errorText.text = error;
            };
            _sequenceLogin.GuestLogin();
        }
    }
}