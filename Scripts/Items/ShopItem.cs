using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sequence.ABI;
using Sequence.Relayer;
using Sequence.Utils;
using Sequence.EmbeddedWallet;
using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "ShopItem", menuName = "Sequence/Game/ShopItem")]
    public class ShopItem : ScriptableObject
    {
        public Item Item;
        public ItemStatus Status { get; private set; }
        public CostItem[] CostItems;
        public Item[] RequiredAchievements;

        public event Action<ItemStatus> OnStatusChanged;

        public static event Action<string> OnFailedToPurchaseShopItem;

        public void SetStatus(ItemStatus status)
        {
            Status = status;
            OnStatusChanged?.Invoke(status);
        }
        
        public async Task Buy()
        {
            if (Status != ItemStatus.Available)
            {
                return;
            }
            
            if (SequenceConnector.Instance == null)
            {
                string error = "SequenceConnector not found. User has not logged in";
                Debug.LogError($"Failed to purchase shop item: {error}");
                OnFailedToPurchaseShopItem?.Invoke($"Failed to purchase shop item: {error}");
                return;
            }

            SequenceConnector.Instance.AddToTransactionQueue(new PurchaseShopItemQueueableTransaction(this));
            TransactionReturn result = await SequenceConnector.Instance.SubmitQueuedTransactions(true, false);
            if (result is SuccessfulTransactionReturn successfulTransactionReturn)
            {
                BurnTokensFromInventory();
                MintTokenInInventory();

                if (string.IsNullOrWhiteSpace(successfulTransactionReturn.txHash))
                {
                    GetTransactionReceipt(successfulTransactionReturn);
                }
            }
            else if (result is FailedTransactionReturn failed)
            {
                string error = $"Transaction failed: {failed.error}";
                Debug.LogError(error);
                OnFailedToPurchaseShopItem?.Invoke($"Failed to purchase shop item: {error}");
            }
            else
            {
                throw new Exception("Unexpected transaction result type");
            }
        }

        private void BurnTokensFromInventory()
        {
            int items = CostItems.Length;
            string[] tokenIds = new string[items];
            BigInteger[] amounts = new BigInteger[items];
            for (int i = 0; i < items; i++)
            {
                tokenIds[i] = CostItems[i].Item.TokenId;
                amounts[i] = CostItems[i].Amount;
            }

            if (SequenceConnector.Instance == null)
            {
                throw new Exception("User is not logged in");
            }
            SequenceConnector.Instance.BurnTokensFromInventoryOnly(tokenIds, amounts);
        }
        
        private void MintTokenInInventory()
        {
            if (SequenceConnector.Instance == null)
            {
                throw new Exception("User is not logged in");
            }
            SequenceConnector.Instance.MintTokensInInventoryOnly(new string[] {Item.TokenId}, new BigInteger[] {BigInteger.One});
        }

        private void GetTransactionReceipt(SuccessfulTransactionReturn successfulTransactionReturn)
        {
            SequenceConnector.Instance.GetTransactionReceipt(successfulTransactionReturn);
        }
    }

    public class PurchaseShopItemQueueableTransaction : IQueueableTransaction
    {
        private ShopItem _shopItem;
        
        public PurchaseShopItemQueueableTransaction(ShopItem shopItem)
        {
            _shopItem = shopItem;
        }
        
        public Transaction BuildTransaction()
        {
            CostItem[] costItems = _shopItem.CostItems;
            int items = costItems.Length;

            SendERC1155Values[] values = new SendERC1155Values[items];
            for (int i = 0; i < items; i++)
            {
                values[i] = new SendERC1155Values(costItems[i].Item.TokenId, costItems[i].Amount.ToString());
            }
            
            NumberCoder numberCoder = new NumberCoder();
            string data = numberCoder.Encode(BigInteger.Parse(_shopItem.Item.TokenId)).ByteArrayToHexStringWithPrefix();
            
            Transaction transaction = new SendERC1155(SequenceConnector.ContractAddress,
                SequenceConnector.BurnToMintContractAddress, values, data);
            return transaction;
        }

        public override string ToString()
        {
            return $"Purchase {_shopItem.Item.Name} with id {_shopItem.Item.TokenId}{RequiredString()} by burning {CostString()}";
        }

        private string RequiredString()
        {
            if (_shopItem.RequiredAchievements == null || _shopItem.RequiredAchievements.Length == 0)
            {
                return "";
            }
            int length = _shopItem.RequiredAchievements.Length;
            StringBuilder result = new StringBuilder(", requiring: ");
            for (int i = 0; i < length; i++)
            {
                result.Append($"{_shopItem.RequiredAchievements[i].Name} with id {_shopItem.RequiredAchievements[i].TokenId}");
                if (i < length - 1)
                {
                    result.Append(", ");
                }
            }

            return result.ToString();
        }

        private string CostString()
        {
            int length = _shopItem.CostItems.Length;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(
                    $"{_shopItem.CostItems[i].Amount} {_shopItem.CostItems[i].Item.Name} with id {_shopItem.CostItems[i].Item.TokenId}");
                if (i < length - 1)
                {
                    result.Append(", ");
                }
            }

            return result.ToString();
        }
    }
    
    public enum ItemStatus
    {
        Available,
        Purchased,
        Locked,
        Equipped,
    }
}