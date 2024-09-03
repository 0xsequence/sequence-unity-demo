using Sequence.Demo;
using Sequence.Utils;
using TMPro;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class AccountLandingPage : UIPage
    {
        [SerializeField] private TextMeshProUGUI _titleText;

        private string _email;

        public override void Open(params object[] args)
        {
            base.Open(args);
            string email = args.GetObjectOfTypeIfExists<string>();
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "Guest";
            }
            _email = email;
            
            _titleText.text = $"ACCOUNT ({_email.ToUpper()})";
        }
    }
}