using Sequence;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class SessionTransactionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _transactionHashText;
        
        private string _transactionHash;
        
        public void Assemble(string transactionHash)
        {
            _transactionHash = transactionHash;
            _transactionHashText.text = CondenseTransactionHashForUI(transactionHash);
        }

        public void ViewTransaction()
        {
            Application.OpenURL(ChainDictionaries.BlockExplorerOf[SequenceConnector.Chain] + $"tx/{_transactionHash}");
        }
        
        private string CondenseTransactionHashForUI(string transactionHash)
        {
            return transactionHash.Substring(0, 6) + "..." + transactionHash.Substring(transactionHash.Length - 4, 4);
        }
    }
}