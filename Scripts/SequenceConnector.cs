using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Sequence;
using Sequence.Contracts;
using Sequence.Relayer;
using Sequence.EmbeddedWallet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Game.Scripts
{
    /// <summary>
    /// Attach this to a GameObject in your scene. It will automatically capture a SequenceWallet when it is created and setup all event handlers (fill in your own logic).
    /// This mono behaviour will persist between scenes and is accessed via SequenceConnector.Instance singleton.
    /// </summary>
    public class SequenceConnector : MonoBehaviour
    {
        private SequenceWalletTransactionQueuer _transactionQueuer;
        private PermissionedMinterTransactionQueuer _permissionedMinterTransactionQueuer;
        
        public const string ContractAddress = "0x93a8c78ff36f656e56d2044e9c925dcd254e51ce"; 
        public const string CollectibleTokenId = "1001";
        public const string BurnToMintContractAddress = "0x079294e6ffec16234578c672fa3fbfd4b6c48640";

        public const Chain Chain = Sequence.Chain.Avalanche;
        public static SequenceConnector Instance { get; private set; }

        public SequenceWallet Wallet { get; private set; }
        public IIndexer Indexer { get; private set; }
        
        public Inventory Inventory { get; private set; }
        
        public ItemCatalogue ItemCatalogue { get; private set; }
        
        public List<string> SessionTransactionHashes { get; private set; }
        
        public Action<string> OnSuccessfulTransaction;

        public Action OnItemPurchasedSuccessfully;

        private const string _mintEndpoint = "https://sequence-relayer-jelly-forest-avalanche.sequence-demos.xyz";
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SequenceWallet.OnWalletCreated += OnWalletCreated;
            Indexer = new ChainIndexer(Chain);
            _transactionQueuer = GetComponent<SequenceWalletTransactionQueuer>();
            _permissionedMinterTransactionQueuer = GetComponent<PermissionedMinterTransactionQueuer>();
            ItemCatalogue = new ItemCatalogue();
            SessionTransactionHashes = new List<string>();
        }

        private void Update()
        {
            if (Inventory != null)
            {
                Inventory.RefreshTokenBalances();
            }
        }

        private void OnWalletCreated(SequenceWallet wallet)
        {
            Wallet = wallet;
            Wallet.OnSendTransactionComplete += OnSendTransactionCompleteHandler;
            Wallet.OnSendTransactionFailed += OnSendTransactionFailedHandler;
            Wallet.OnSignMessageComplete += OnSignMessageCompleteHandler;
            Wallet.OnDeployContractComplete += OnDeployContractCompleteHandler;
            Wallet.OnDeployContractFailed += OnDeployContractFailedHandler;
            Wallet.OnDropSessionComplete += OnDropSessionCompleteHandler;
            Wallet.OnSessionsFound += OnSessionsFoundHandler;
            
            Inventory = new Inventory(Indexer, Wallet.GetWalletAddress(), ItemCatalogue);
            
            _transactionQueuer.Setup(Wallet, Chain);
            _permissionedMinterTransactionQueuer.Setup(Wallet, Chain, _mintEndpoint, ContractAddress);
        }

        private void OnDestroy()
        {
            if (Wallet == null) return;
            ResetWallet();
            SequenceWallet.OnWalletCreated -= OnWalletCreated;
            SceneManager.LoadScene("LoginScene");
        }

        private void ResetWallet()
        {
            if (Wallet == null) return;
            Wallet.OnSendTransactionComplete -= OnSendTransactionCompleteHandler;
            Wallet.OnSendTransactionFailed -= OnSendTransactionFailedHandler;
            Wallet.OnSignMessageComplete -= OnSignMessageCompleteHandler;
            Wallet.OnDeployContractComplete -= OnDeployContractCompleteHandler;
            Wallet.OnDeployContractFailed -= OnDeployContractFailedHandler;
            Wallet.OnDropSessionComplete -= OnDropSessionCompleteHandler;
            Wallet.OnSessionsFound -= OnSessionsFoundHandler;
            Wallet = null;
        }

        private void OnSendTransactionCompleteHandler(SuccessfulTransactionReturn result) {
            if (string.IsNullOrWhiteSpace(result.txHash))
            {
                return; // This will be fetched later
            }
            Debug.Log($"Transaction succeeded. Transaction hash: {result.txHash} | Request: {result.request}");
            SessionTransactionHashes.Add(result.txHash);
            OnSuccessfulTransaction?.Invoke(result.txHash);
        }

        public async Task GetTransactionReceipt(SuccessfulTransactionReturn successfulTransactionReturn)
        {
            SuccessfulTransactionReturn withTransactionHash = await Wallet.WaitForTransactionReceipt(successfulTransactionReturn);
            OnSendTransactionCompleteHandler(withTransactionHash);
        }

        private void OnSendTransactionFailedHandler(FailedTransactionReturn result) {
            Debug.LogError($"Transaction failed. Reason: {result.error}");
        }
        
        private void OnSignMessageCompleteHandler(string result) {
            // Do something
        }
        
        private void OnDeployContractCompleteHandler(SuccessfulContractDeploymentReturn result) {
            Address newlyDeployedContractAddress = result.DeployedContractAddress;

            // Do something
        }

        private void OnDeployContractFailedHandler(FailedContractDeploymentReturn result) {
            // Do something
        }
        
        private void OnDropSessionCompleteHandler(string sessionId) {
            if (sessionId == Wallet.SessionId)
            {
                ResetWallet();
                SequenceLogin.GetInstance().SetConnectedWalletAddress(null);
                SceneManager.LoadScene("LoginScene");
            }
        }
        
        private void OnSessionsFoundHandler(Session[] sessions) {
            // Do something
        }

        public void MintFungibleToken(uint amount = 1, bool mintInInventory = true)
        {
            _permissionedMinterTransactionQueuer.Enqueue(new PermissionedMintTransaction(CollectibleTokenId, amount));
            if (mintInInventory)
            {
                Inventory.MintToken(CollectibleTokenId, amount);
            }
        }

        public async Task<TransactionReturn> SubmitQueuedTransactions(bool overrideWait = false, bool waitForReceipt = true)
        {
            PermissionedMinterQueueSubmissionResult mintResult = await _permissionedMinterTransactionQueuer.SubmitTransactions(overrideWait);
            if (mintResult.Success)
            {
                int transactions = mintResult.TransactionHashes.Length;
                for (int i = 0; i < transactions; i++)
                {
                    SessionTransactionHashes.Add(mintResult.TransactionHashes[i]);
                    OnSuccessfulTransaction?.Invoke(mintResult.TransactionHashes[i]);
                }
            }
            TransactionReturn result = await _transactionQueuer.SubmitTransactions(overrideWait, waitForReceipt);
            if (result == null && mintResult.Success)
            {
                SuccessfulTransactionReturn[] successfulTransactionReturns = new SuccessfulTransactionReturn[mintResult.TransactionHashes.Length];
                for (int i = 0; i < mintResult.TransactionHashes.Length; i++)
                {
                    successfulTransactionReturns[i] = new SuccessfulTransactionReturn(mintResult.TransactionHashes[i], "", null, null);
                }

                return new SuccessfulBatchTransactionReturn(successfulTransactionReturns);
            }
            return result;
        }
        
        public void AddToTransactionQueue(IQueueableTransaction transaction)
        {
            _transactionQueuer.Enqueue(transaction);
        }

        public void AddToPremiumTransactionQueue(PermissionedMintTransaction payload, string iapReceipt)
        {
            PremiumIAPMinter minter = new PremiumIAPMinter(new MintingRequestProver(Wallet, Chain),
                _mintEndpoint, ContractAddress, iapReceipt);
            _permissionedMinterTransactionQueuer.Enqueue((payload, minter));
        }

        public void DebugAddTokens()
        {
            MintFungibleToken(100);
        }

        public void BurnTokensFromInventoryOnly(string[] tokenIds, BigInteger[] amounts)
        {
            int items = tokenIds.Length;
            if (items != amounts.Length)
            {
                throw new ArgumentException($"{nameof(tokenIds)} and {nameof(amounts)} must be the same length");
            }

            for (int i = 0; i < items; i++)
            {
                Inventory.BurnToken(tokenIds[i], amounts[i]);
            }
        }
        
        public void MintTokensInInventoryOnly(string[] tokenIds, BigInteger[] amounts)
        {
            int items = tokenIds.Length;
            if (items != amounts.Length)
            {
                throw new ArgumentException($"{nameof(tokenIds)} and {nameof(amounts)} must be the same length");
            }

            for (int i = 0; i < items; i++)
            {
                Inventory.MintToken(tokenIds[i], amounts[i]);
            }
        }

        public void Logout()
        {
            Wallet.DropThisSession();
        }

        public void LinkEOA()
        {
            Application.OpenURL("https://demo-waas-wallet-link.pages.dev/");
        }

        public BigInteger GetQueuedMintAmount()
        {
            string queued = _permissionedMinterTransactionQueuer.ToString();
            if (queued == "0 Queued Transactions")
            {
                return 0;
            }
            
            string[] split = queued.Split(':');
            string[] transactions = split[1].Split('|');
            int transactionsCount = transactions.Length;
            BigInteger total = 0;
            string filler = " Mint ";
            for (int i = 0; i < transactionsCount; i++)
            {
                string amount = transactions[i].Substring(filler.Length, transactions[i].IndexOf(" of") - filler.Length).Trim();
                string id = transactions[i].Substring(transactions[i].IndexOf("Token Id") + "Token Id".Length).Trim();
                id = id.Substring(0, id.IndexOf(','));
                if (id == CollectibleTokenId)
                {
                    total += BigInteger.Parse(amount);
                }
            }
            
            return total;
        }
    }
}