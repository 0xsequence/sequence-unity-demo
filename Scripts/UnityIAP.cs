using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Game.Scripts
{
    public class UnityIAP : IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        
        private Dictionary<string, Product> _productsToBeFulfilled = new Dictionary<string, Product>();

        public bool IsInitialized { get; private set; } = false;

        public Action<IAPItemReadyForFulfillment> OnReadyForFulfillment;
        public Action<string> OnFailedToPurchase;
        
        private static UnityIAP _instance;
        public string InitializationError = "Failed to initialize Unity IAP: timeout; please double-check your network connection and try again.";

        private UnityIAP()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct("10190", ProductType.NonConsumable, new IDs
            {
                { "10190", GooglePlay.Name },
                { "10190", AppleAppStore.Name }
            });

            StandardPurchasingModule.Instance().useFakeStoreAlways = true;
            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;
            
            UnityPurchasing.Initialize(this, builder);
        }

        public static UnityIAP GetInstance()
        {
            if (_instance == null)
            {
                _instance = new UnityIAP();
            }

            return _instance;
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            string errorMessage = "Failed to initialize Unity IAP: " + error;
            InitializationError = errorMessage;
            Debug.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            _productsToBeFulfilled[purchaseEvent.purchasedProduct.definition.id] = purchaseEvent.purchasedProduct;
            OnReadyForFulfillment?.Invoke(new IAPItemReadyForFulfillment(purchaseEvent.purchasedProduct.definition.id, purchaseEvent.purchasedProduct.receipt));
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string error = $"Failed to purchase product {product.definition.id}: {failureReason}";
            Debug.LogError(error);
            OnFailedToPurchase?.Invoke(error);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            
            foreach (var product in controller.products.all) {
                Debug.Log (product.metadata.localizedTitle);
                Debug.Log (product.metadata.localizedDescription);
                Debug.Log (product.metadata.localizedPriceString);
            }

            IsInitialized = true;
        }

        public void PurchaseProduct(string productId)
        {
            _controller.InitiatePurchase(productId);
        }
        
        public void OnPurchaseFulfilled(string productId)
        {
            _controller.ConfirmPendingPurchase(_productsToBeFulfilled[productId]);
            _productsToBeFulfilled.Remove(productId);
        }
        
        public string GetPriceString(string productId)
        {
            Product product = _controller.products.WithID(productId);
            string price = product.metadata.localizedPriceString;
            return price;
        }
    }

    [Serializable]
    public class IAPItemReadyForFulfillment
    {
        public string ProductId;
        public string Receipt;

        public IAPItemReadyForFulfillment(string productId, string receipt)
        {
            ProductId = productId;
            Receipt = receipt;
        }
    }
}