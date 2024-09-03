using Sequence.Demo;

namespace Game.Scripts.UI
{ 
    public class AccountPanel : UIPanel
    {
        private SessionTransactionsPage _sessionTransactionsPage;
        
        protected override void Awake()
        {
            base.Awake();
            _sessionTransactionsPage = GetComponentInChildren<SessionTransactionsPage>();
        }
        
        public void OpenSessionTransactionsPage()
        {
            StartCoroutine(SetUIPage(_sessionTransactionsPage));
        }
    }
}