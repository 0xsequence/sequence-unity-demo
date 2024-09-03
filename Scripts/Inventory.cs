using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Sequence;
using Sequence.Contracts;
using Sequence.EmbeddedWallet;
using UnityEngine;

namespace Game.Scripts
{
    public class Inventory
    {
        public Action<InventoryBalanceChanged> OnInventoryBalanceChanged;

        private IIndexer _indexer;
        private Address _userAddress;
        private Dictionary<BigInteger, TokenBalance> _tokenBalances = new Dictionary<BigInteger, TokenBalance>();
        
        private ItemCatalogue _itemCatalogue;
        
        private List<Address> _mintedStarterTokensTo = new List<Address>();

        public Inventory(IIndexer indexer, Address userAddress, ItemCatalogue itemCatalogue)
        {
            _indexer = indexer;
            _userAddress = userAddress;
            GetTokenBalances();
            _itemCatalogue = itemCatalogue;
        }

        public void RefreshTokenBalances()
        {
            GetTokenBalances();
        }
        
        private async Task GetTokenBalances(Page page = null)
        {
            if (page == null)
            {
                page = new Page();
            }
            GetTokenBalancesReturn balances = await _indexer.GetTokenBalances(new GetTokenBalancesArgs(_userAddress, SequenceConnector.ContractAddress, false, page));
            int uniqueTokens = balances.balances.Length;
            if (uniqueTokens == 0)
            {
                MintStarterTokens();
            }
            for (int i = 0; i < uniqueTokens; i++)
            {
                _tokenBalances[balances.balances[i].tokenID] = balances.balances[i];
            }
            
            if (balances.page.more)
            {
                await GetTokenBalances(balances.page);
            }
        }

        private async Task MintStarterTokens()
        {
            Address userAddress = SequenceConnector.Instance.Wallet.GetWalletAddress();
            if (_mintedStarterTokensTo.Contains(userAddress))
            {
                return;
            }
            _mintedStarterTokensTo.Add(userAddress);
            SequenceConnector.Instance.MintFungibleToken(300, false);
            await SequenceConnector.Instance.SubmitQueuedTransactions(true);
            SequenceConnector.Instance.MintTokensInInventoryOnly(new []{ SequenceConnector.CollectibleTokenId }, new BigInteger[] { 300 });
        }

        public void MintToken(string tokenId, BigInteger amount)
        {
            BigInteger tokenIdBigInt = BigInteger.Parse(tokenId);
            if (_tokenBalances.ContainsKey(tokenIdBigInt))
            {
                _tokenBalances[tokenIdBigInt].balance += amount;
            }
            else
            {
                _tokenBalances[tokenIdBigInt] = new TokenBalance
                {
                    contractAddress = SequenceConnector.ContractAddress,
                    accountAddress = _userAddress,
                    tokenID = tokenIdBigInt,
                    balance = amount,
                    chainId = _indexer.GetChainID()
                };
            }
            
            OnInventoryBalanceChanged?.Invoke(new InventoryBalanceChanged(
                tokenId, 
                amount, 
                _tokenBalances[tokenIdBigInt].balance));
        }
        
        public void BurnToken(string tokenId, BigInteger amount)
        {
            BigInteger tokenIdBigInt = BigInteger.Parse(tokenId);
            if (_tokenBalances.ContainsKey(tokenIdBigInt))
            {
                _tokenBalances[tokenIdBigInt].balance -= amount;
                OnInventoryBalanceChanged?.Invoke(new InventoryBalanceChanged(
                    tokenId, 
                    -amount, 
                    _tokenBalances[tokenIdBigInt].balance));
                if (_tokenBalances[tokenIdBigInt].balance == 0)
                {
                    _tokenBalances.Remove(tokenIdBigInt);
                }
            }
            else
            {
                throw new Exception("Token not found in inventory");
            }
        }

        public uint GetTokens()
        {
            BigInteger tokenId = BigInteger.Parse(SequenceConnector.CollectibleTokenId);
            if (_tokenBalances.TryGetValue(tokenId , out var tokenBalance))
            {
                return (uint) tokenBalance.balance;
            }
            else
            {
                return 0;
            }
        }

        public bool HasNTokensOfId(BigInteger n, string tokenId)
        {
            BigInteger tokenIdBigInt = BigInteger.Parse(tokenId);
            if (_tokenBalances.TryGetValue(tokenIdBigInt, out var tokenBalance))
            {
                return tokenBalance.balance >= n;
            }
            else
            {
                return false;
            }
        }
        
        public bool HasTokenWithId(string tokenId)
        {
            BigInteger tokenIdBigInt = BigInteger.Parse(tokenId);
            return _tokenBalances.ContainsKey(tokenIdBigInt);
        }
        
        /// <summary>
        /// Returns the tier level of the highest tiered item of the given power up type that is held in the user's inventory.
        /// </summary>
        /// <param name="powerUpType"></param>
        /// <returns></returns>
        public uint GetBestTieredItem(ItemCatalogue.PowerUpType powerUpType)
        {
            List<Item> validItems = _itemCatalogue.ItemsByPowerUpType[powerUpType];
            uint bestTier = 0;
            int items = validItems.Count;
            for (int i = 0; i < items; i++)
            {
                Item item = validItems[i];
                if (HasTokenWithId(item.TokenId))
                {
                    bestTier = Math.Max(bestTier, item.Tier);
                }
            }

            return bestTier;
        }

        public async Task ResetSave()
        {
            GetTokenBalancesReturn balances = await _indexer.GetTokenBalances(new GetTokenBalancesArgs(_userAddress, SequenceConnector.ContractAddress, false));
            Transaction[] transactions = new Transaction[balances.balances.Length];
            
            ERC1155 tokens = new ERC1155(SequenceConnector.ContractAddress);
            for (int i = 0; i < balances.balances.Length; i++)
            {
                transactions[i] = new RawTransaction(tokens.Burn(balances.balances[i].tokenID, balances.balances[i].balance));
            }

            if (transactions.Length > 0)
            {
                var result = await SequenceConnector.Instance.Wallet.SendTransaction(SequenceConnector.Chain, transactions);
                if (result is FailedTransactionReturn failed)
                {
                    throw new Exception($"Failed to reset save: {failed.error}");
                }
                else
                {
                    var success = (SuccessfulTransactionReturn) result;
                    Application.OpenURL(ChainDictionaries.BlockExplorerOf[SequenceConnector.Chain] + "tx/" + success.txHash);
                }
            }
            
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            await GetTokenBalances();
            
            Debug.Log("Finished resetting game");
        }
    }

    public class InventoryBalanceChanged
    {
        public string TokenId;
        public BigInteger IncreasedByAmount;
        public BigInteger NewAmount;

        public InventoryBalanceChanged(string tokenId, BigInteger increasedByAmount, BigInteger newAmount)
        {
            TokenId = tokenId;
            IncreasedByAmount = increasedByAmount;
            NewAmount = newAmount;
        }
    }
}