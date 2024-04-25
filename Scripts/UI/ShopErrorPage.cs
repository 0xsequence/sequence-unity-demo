using System;
using Sequence.Demo;

namespace Game.Scripts
{
    public class ShopErrorPage : UIPage
    {
        private UIPanel _parent;
        
        protected override void Awake()
        {
            base.Awake();
            ShopItem.OnFailedToPurchaseShopItem += HandleFailedToPurchaseShopItem;
            _parent = FindObjectOfType<UIPanel>(); // The parent of this UIPage doesn't matter as it handles its own opening and closing
        }
        
        private void HandleFailedToPurchaseShopItem(string errorMessage)
        {
            Open(_parent);
        }

        protected void OnDestroy()
        {
            ShopItem.OnFailedToPurchaseShopItem -= HandleFailedToPurchaseShopItem;
        }
    }
}