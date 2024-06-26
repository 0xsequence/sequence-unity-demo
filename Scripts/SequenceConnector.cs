using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Sequence;
using Sequence.Contracts;
using Sequence.Relayer;
using Sequence.WaaS;
using Sequence.WaaS.Authentication;
using SequenceSDK.WaaS;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Game.Scripts
{
    /// <summary>
    /// Attach this to a GameObject in your scene. It will automatically capture a WaaSWallet when it is created and setup all event handlers (fill in your own logic).
    /// This mono behaviour will persist between scenes and is accessed via SequenceConnector.Instance singleton.
    /// </summary>
    public class SequenceConnector : MonoBehaviour
    {
        private WaaSTransactionQueuer _transactionQueuer;
        private PermissionedMinterTransactionQueuer _permissionedMinterTransactionQueuer;
        
        public const string ContractAddress = "0x32d70df2b156242f1b19f60fa40d05f8966244ec"; 
        public const string CollectibleTokenId = "1001";
        public const string BurnToMintContractAddress = "0xEC96AD8eb0DBba71c6f218344450e4Bd30D7d584";

        public const Chain Chain = Sequence.Chain.ArbitrumNova;
        public static SequenceConnector Instance { get; private set; }

        public WaaSWallet Wallet { get; private set; }
        public IIndexer Indexer { get; private set; }
        
        public Inventory Inventory { get; private set; }
        
        public ItemCatalogue ItemCatalogue { get; private set; }
        
        public List<string> SessionTransactionHashes { get; private set; }
        
        public Action<string> OnSuccessfulTransaction;
        
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
            }

            WaaSWallet.OnWaaSWalletCreated += OnWaaSWalletCreated;
            Indexer = new ChainIndexer(Chain);
            _transactionQueuer = GetComponent<WaaSTransactionQueuer>();
            _permissionedMinterTransactionQueuer = GetComponent<PermissionedMinterTransactionQueuer>();
            ItemCatalogue = new ItemCatalogue();
            SessionTransactionHashes = new List<string>();
        }

        private void OnWaaSWalletCreated(WaaSWallet wallet)
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
            _permissionedMinterTransactionQueuer.Setup(Wallet, Chain, "https://sequence-relayer-jelly-forest2.tpin.workers.dev/", ContractAddress);
        }

        private void OnDestroy()
        {
            if (Wallet == null) return;
            Wallet.OnSendTransactionComplete -= OnSendTransactionCompleteHandler;
            Wallet.OnSendTransactionFailed -= OnSendTransactionFailedHandler;
            Wallet.OnSignMessageComplete -= OnSignMessageCompleteHandler;
            Wallet.OnDeployContractComplete -= OnDeployContractCompleteHandler;
            Wallet.OnDeployContractFailed -= OnDeployContractFailedHandler;
            Wallet.OnDropSessionComplete -= OnDropSessionCompleteHandler;
            Wallet.OnSessionsFound -= OnSessionsFoundHandler;
            Wallet.DropThisSession();
            SceneManager.LoadScene("LoginScene");
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
            // Do something
        }
        
        private void OnSessionsFoundHandler(WaaSSession[] sessions) {
            // Do something
        }

        public void MintFungibleToken(uint amount = 1)
        {
            _permissionedMinterTransactionQueuer.Enqueue(new PermissionedMintTransaction(CollectibleTokenId, amount));
            Inventory.MintToken(CollectibleTokenId, amount);
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
            return result;
        }
        
        public void AddToTransactionQueue(IQueueableTransaction transaction)
        {
            _transactionQueuer.Enqueue(transaction);
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
            Instance = null;
            Destroy(gameObject);
        }
    }
}