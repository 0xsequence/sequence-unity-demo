using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Sequence.Demo;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public abstract class ShopPage : UIPage
    {
        [SerializeField] private TextMeshProUGUI _tokensAmountText;
        [SerializeField] protected ShopItem[] _shopItems;
        [SerializeField] private float _timeBetweenShopItemRefreshesInSeconds = 60f;
        
        protected Inventory _inventory;
        protected Dictionary<string, ShopItem> _shopItemsById = new Dictionary<string, ShopItem>();
        protected ItemCatalogue _itemCatalogue;
        private bool _hasRefreshedShop = false;

        protected override void Awake()
        {
            base.Awake();
            _itemCatalogue = new ItemCatalogue();
        }

        public override void Open(params object[] args)
        {
            base.Open(args);

            if (SequenceConnector.Instance)
            {
                _inventory = SequenceConnector.Instance.Inventory;
                SetTokenAmountsText();
            }
            else
            {
                Debug.LogWarning("SequenceConnector not found. Shop will not work properly as user has not logged in.");
            }

            InvokeRepeating(nameof(RefreshShopItems), 0, _timeBetweenShopItemRefreshesInSeconds);
            
            UpdateItemStatuses();
            PopulateShop();

            if (_inventory != null)
            {
                _inventory.OnInventoryBalanceChanged += HandleInventoryBalanceChanged;
            }
            else
            {
                Debug.LogWarning("Inventory is null. Balances will not get updated");
            }
        }

        private void SetTokenAmountsText()
        {
            _tokensAmountText.text = _inventory.GetTokens().ToString();
        }
        
        private void HandleInventoryBalanceChanged(InventoryBalanceChanged balanceChanged)
        {
            if (balanceChanged.TokenId == SequenceConnector.CollectibleTokenId)
            {
                SetTokenAmountsText();
            }
            UpdateItemStatuses();
            RefreshShopItemUIs();
        }

        public override void Close()
        {
            _animator.AnimateOut(_closeAnimationDurationInSeconds);
            Cleanup();
            _gameObject.SetActive(false);
            Invoke(nameof(Deactivate), _closeAnimationDurationInSeconds);
        }

        private void Deactivate()
        {
            _gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryBalanceChanged -= HandleInventoryBalanceChanged;
            }
            CancelInvoke(nameof(RefreshShopItems));
        }

        protected abstract void PopulateShop();
        
         private void UpdateItemStatuses()
        {
            if (_inventory == null)
            {
                Debug.LogWarning("Inventory is null. This likely means that the user has not logged in. Item statuses will not be updated.");
                return;
            }
            
            int shopItems = _shopItems.Length;
            for (int i = 0; i < shopItems; i++)
            {
                UpdateItemStatus(_shopItems[i]);
            }
        }

        private void UpdateItemStatus(ShopItem shopItem)
        {
            Item item = shopItem.Item;
            
            if (item.PowerUpType == ItemCatalogue.PowerUpType.Skin && shopItem.Status == ItemStatus.Equipped)
            {
                return;
            }
            
            if (_inventory.HasTokenWithId(item.TokenId))
            {
                ItemStatus status = ItemStatus.Purchased;
                if (item.PowerUpType == ItemCatalogue.PowerUpType.Skin && item.IsSkinEquipped())
                {
                    status = ItemStatus.Equipped;
                }
                shopItem.SetStatus(status);
                return;
            }
                
            Item[] requiredAchievements = shopItem.RequiredAchievements;
            int requiredAchievementsCount = requiredAchievements.Length;
            bool allAchievementsUnlocked = true;
            for (int j = 0; j < requiredAchievementsCount; j++)
            {
                if (!_inventory.HasTokenWithId(requiredAchievements[j].TokenId))
                {
                    allAchievementsUnlocked = false;
                    break;
                }
            }
            if (!allAchievementsUnlocked)
            {
                shopItem.SetStatus(ItemStatus.Locked);
                return;
            }
                
            CostItem[] costItems = shopItem.CostItems;
            int costItemsCount = costItems.Length;
            bool canAfford = true;
            for (int j = 0; j < costItemsCount; j++)
            {
                try
                {
                    if (!_inventory.HasNTokensOfId(costItems[j].Amount, costItems[j].Item.TokenId))
                    {
                        canAfford = false;
                        costItems[j].SetCanAfford(false);
                    }
                    else
                    {
                        costItems[j].SetCanAfford(true);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error checking cost item {costItems[j].Item.Name} with id {costItems[j].Item.TokenId}: {e.Message}");
                    shopItem.SetStatus(ItemStatus.Locked);
                    return;
                }
            }
            
            if (canAfford)
            {
                shopItem.SetStatus(ItemStatus.Available);
            }
            else
            {
                shopItem.SetStatus(ItemStatus.Locked);
            }
        }

        public void DebugAddTokens()
        {
            if (SequenceConnector.Instance == null)
            {
                Debug.LogError("User is not logged in. Cannot add tokens.");
            }
            
            SequenceConnector.Instance.DebugAddTokens();
        }
        
        private async Task RefreshShopItems()
        {
            try
            {
                BurnToMint burnToMint = new BurnToMint();
                int shopItems = _shopItems.Length;
                for (int i = 0; i < shopItems; i++)
                {
                    Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]> mintingRequirements = 
                        await burnToMint.GetMintingRequirements(_shopItems[i].Item.TokenId);
                    int costItems = mintingRequirements.Item1.Length;
                    int requiredAchievements = mintingRequirements.Item3.Length;
                    
                    CostItem[] costItemsArray = new CostItem[costItems];
                    for (int j = 0; j < costItems; j++)
                    {
                        costItemsArray[j] =
                            CostItem.Create(_itemCatalogue.ItemsByTokenId[mintingRequirements.Item1[j].ToString()],
                                (int)mintingRequirements.Item2[j]);
                    }

                    DestroyCostItems(_shopItems[i]);

                    _shopItems[i].CostItems = costItemsArray;
                    
                    Item[] requiredAchievementsArray = new Item[requiredAchievements];
                    for (int j = 0; j < requiredAchievements; j++)
                    {
                        requiredAchievementsArray[j] = _itemCatalogue.ItemsByTokenId[mintingRequirements.Item3[j].ToString()];
                    }
                    _shopItems[i].RequiredAchievements = requiredAchievementsArray;
                }

                UpdateItemStatuses();
                RefreshShopItemUIs();
                _hasRefreshedShop = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error refreshing shop items: {e.Message}");
            }
        }
        
        private void DestroyCostItems(ShopItem shopItem)
        {
            if (!_hasRefreshedShop)
            {
                return; // When Unity reloads the scene, it will use the original ShopItems. If we delete their CostItems we will cause a null reference exception.
            }
            
            int costItems = shopItem.CostItems.Length;
            for (int i = 0; i < costItems; i++)
            {
                Destroy(shopItem.CostItems[i]);
            }

            shopItem.CostItems = null;
        }
        
        protected abstract void RefreshShopItemUIs();
    }
}