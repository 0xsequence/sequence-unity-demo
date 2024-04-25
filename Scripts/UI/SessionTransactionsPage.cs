using System.Collections.Generic;
using Sequence.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Game.Scripts
{
    public class SessionTransactionsPage : UIPage
    {
        [SerializeField] private GameObject _transactionPrefab;
        [SerializeField] private VerticalLayoutGroup _transactionsLayoutGroup;
        [SerializeField] private TextMeshProUGUI _transactionsCountText;
        [SerializeField] private ScrollRect _scrollView;
        [SerializeField] private GameObject _divider;
        [SerializeField] private bool _spawnDividers = false;
        
        private RectTransform _transactionsContainer;
        private int _transactionsCount;
        private List<SessionTransactionUI> _transactions = new List<SessionTransactionUI>();
        private RectTransform _transactionsLayoutGroupRectTransform;
        private List<GameObject> _dividers = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            _transactionsContainer = _transactionsLayoutGroup.GetComponent<RectTransform>();
            _transactionsLayoutGroupRectTransform = _transactionsLayoutGroup.GetComponent<RectTransform>();
        }

        public override void Open(params object[] args)
        {
            base.Open(args);
#if !UNITY_EDITOR
            if (SequenceConnector.Instance == null)
            {
                Debug.LogError("SequenceConnector not found - user likely has not logged in");
                return;
            }

            SequenceConnector.Instance.OnSuccessfulTransaction += AddNewTransaction;
#endif
            
            Populate();
        }

        public override void Close()
        {
            base.Close();
            Cleanup();
        }
        
        private void Cleanup()
        {
            SequenceConnector.Instance.OnSuccessfulTransaction -= AddNewTransaction;
            DestroyExistingTransactionUIs();
        }

        private void Populate()
        {
#if !UNITY_EDITOR
            List<string> transactions = SequenceConnector.Instance.SessionTransactionHashes;
#else
            List<string> transactions = new List<string>();
            transactions.Add("0x869f484826b4b4b8befa04ef0b4bba1f84adf26c4c5f18d904a91847a6b5e321");
            transactions.Add("0xfc9d1f15f1921e9ec3e2dc21ec14b6afebdca5a54cc7ec840c082bbfe4f16409");
            transactions.Add("0xdd139ce0ae02c66ca276b94de03c15b434a769a88020307fabad719a0c1068b3");
            transactions.Add("0x0c27f4586adf0ef821246f6d1780436ccb0129dc403f523631b663e3f9fc793e");
            transactions.Add("0xfd409dbda812a415bd77a937f2d4e41cf175e8b635771cd863120dca916233dd");
            transactions.Add("0x94d8a5ecfc2dcb4dcdb4d792c188e0410c1224369c16302a9571ec827423e0bb");
            transactions.Add("0x473eefe7bc0c414f15e6a916e639379d3b8406cb0205ff8a9ddef458e13cb485");
            transactions.Add("0x6c5c24baa5377fe95b04108a85e1b3b3c74cb1986bb888fd308991ab6881c1b2");
            transactions.Add("0x2b52302b8b97839620cd8141669cbc365d35ecced5ec465d7afab545c2d1df1d");
            transactions.Add("0xa736f1680068c8468105b241229bb439f2aa2ce61f8b062a541f57da7a980c6c");
#endif
           _transactionsCount = transactions.Count;
           SetTransactionCountText();
            for (int i = 0; i < _transactionsCount; i++)
            {
                GameObject transaction = Instantiate(_transactionPrefab, _transactionsContainer);
                SessionTransactionUI transactionUI = transaction.GetComponentInChildren<SessionTransactionUI>();
                transactionUI.Assemble(transactions[i]);
                _transactions.Add(transactionUI);
                
                if (i < _transactionsCount - 1 && _spawnDividers)
                {
                    GameObject divider = Instantiate(_divider, _transactionsContainer);
                    _dividers.Add(divider);
                }
            }
            Invoke(nameof(UpdateScrollView), 0.1f); // Sometimes Instantiate takes too long to complete and we don't yet have children in the layout group
        }
        
        private void DestroyExistingTransactionUIs()
        {
            int transactionsCount = _transactions.Count;
            for (int i = 0; i < transactionsCount; i++)
            {
                Destroy(_transactions[i].gameObject);
            }
            _transactions = new List<SessionTransactionUI>();
            
            int dividersCount = _dividers.Count;
            for (int i = 0; i < dividersCount; i++)
            {
                Destroy(_dividers[i]);
            }
            _dividers = new List<GameObject>();
        }

        private void AddNewTransaction(string transactionHash)
        {
            _transactionsCount++;
            SetTransactionCountText();
            GameObject transaction = Instantiate(_transactionPrefab, _transactionsContainer);
            transaction.GetComponentInChildren<SessionTransactionUI>().Assemble(transactionHash);
        }

        private void SetTransactionCountText()
        {
            _transactionsCountText.text = $"{_transactionsCount}";
        }
        
        private void UpdateScrollView()
        {
            float contentHeight = _transactionsLayoutGroup.preferredHeight;
            _transactionsLayoutGroupRectTransform.sizeDelta =
                new Vector2(_transactionsLayoutGroupRectTransform.sizeDelta.x, contentHeight);
            
            Invoke(nameof(SetScrollViewToTop), .2f);
        }

        private void SetScrollViewToTop()
        {
            _scrollView.verticalScrollbar.value = 1;
        }
    }
}