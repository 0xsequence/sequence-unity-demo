using System;
using System.Threading.Tasks;
using Sequence.EmbeddedWallet;
using Sequence.Relayer;
using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "PremiumItem", menuName = "Sequence/Game/PremiumItem")]
    public class PremiumItem : ShopItem
    {
        private bool _readyForFulfillment = false;
        private UnityIAP _iap;
        private string _receipt;
        
        protected override async Task AddToQueue()
        {
#if UNITY_IOS || UNITY_ANDROID
            _iap = UnityIAP.GetInstance();
            if (!_iap.IsInitialized)
            {
                string error = $"Failed to purchase item {Item.TokenId}: {_iap.InitializationError}";
                FailedToPurchase(error);
                throw new Exception(error);
            }

            _iap.OnReadyForFulfillment += itemReady =>
            {
                if (itemReady.ProductId == Item.TokenId)
                {
                    _receipt = itemReady.Receipt;
                    _readyForFulfillment = true;
                }
            };
            _iap.OnFailedToPurchase += error =>
            {
                FailedToPurchase(error);
                Task.FromException(new Exception(error));
            };
            
            _iap.PurchaseProduct(Item.TokenId);

            while (!_readyForFulfillment)
            {
                await Task.Yield();
            }
            
            SequenceConnector.Instance.AddToPremiumTransactionQueue(new PermissionedMintTransaction(Item.TokenId, 1), _receipt);
#else
            await base.AddToQueue();
#endif
        }

        public static event Action<string> OnFailedToPurchaseShopItem;
        private void FailedToPurchase(string error)
        {
            string errorMessage = $"Failed to purchase premium item {Item.TokenId}: {error}";
            Debug.LogError(errorMessage);
            OnFailedToPurchaseShopItem?.Invoke(errorMessage);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            _readyForFulfillment = false;
            _iap.OnPurchaseFulfilled(Item.TokenId);
        }

        public string GetPriceString()
        {
            if (_iap == null)
            {
                _iap = UnityIAP.GetInstance();
            }
            return _iap.GetPriceString(Item.TokenId);
        }
    }
}