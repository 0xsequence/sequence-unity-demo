using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.Scripts
{
    public class BuyEquipButton
    {
        private ShopItem _shopItem;
        private GameObject _shopLoadingScreenPrefab;
        private Transform _loadingScreenParent;
        private TextMeshProUGUI _buyEquipButtonText;
        private Button _buyEquipButton;
        private ItemStatus _status;
        private GameObject _buyEquipButtonCostItem;
        private Color _defaultButtonColor; 
        private Color _buyButtonColor;
        private Image _buttonImage;
        
        private Dictionary<ItemStatus, string> ButtonTexts = new Dictionary<ItemStatus, string>
        {
            {ItemStatus.Purchased, "Owned"},
            {ItemStatus.Available, "Buy"},
            {ItemStatus.Locked, "Locked"},
            {ItemStatus.Equipped, "Unequip"}
        };
        
        public BuyEquipButton(ShopItem shopItem, GameObject shopLoadingScreenPrefab, Transform loadingScreenParent, TextMeshProUGUI buyEquipButtonText, Button buyEquipButton, ItemStatus status, GameObject buyEquipButtonCostItem, Color defaultButtonColor, Color buyButtonColor)
        {
            _shopItem = shopItem;
            _shopLoadingScreenPrefab = shopLoadingScreenPrefab;
            _loadingScreenParent = loadingScreenParent;
            _buyEquipButtonText = buyEquipButtonText;
            _buyEquipButton = buyEquipButton;
            _status = status;
            _buyEquipButtonCostItem = buyEquipButtonCostItem;
            _defaultButtonColor = defaultButtonColor;
            _buyButtonColor = buyButtonColor;
            _buttonImage = _buyEquipButton.GetComponent<Image>();
            shopItem.OnStatusChanged += HandleStatusChange;
            SetupBuyEquipButton(status);
        }
        
        public void Buy()
        {
            if (_shopItem.Status == ItemStatus.Available)
            {
                DoBuy();
            }
            else if (_shopItem.Status == ItemStatus.Purchased)
            {
                Equip();
            }
            else if (_shopItem.Status == ItemStatus.Equipped)
            {
                Unequip();
            }
            else
            {
                throw new Exception($"Invalid item status: {_shopItem.Status}. Button should have been disabled.");
            }
        }

        private async Task DoBuy()
        {
            GameObject loadingScreen = Object.Instantiate(_shopLoadingScreenPrefab, _loadingScreenParent);
            await _shopItem.Buy();
            Object.Destroy(loadingScreen);
        }

        private void Equip()
        {
            _shopItem.SetStatus(ItemStatus.Equipped);
        }
        
        private void Unequip()
        {
            _shopItem.SetStatus(ItemStatus.Purchased);
        }

        private void SetupBuyEquipButton(ItemStatus status)
        {
            if (_shopItem.Item.PowerUpType == ItemCatalogue.PowerUpType.Skin)
            {
                SetupBuyEquipButtonForSkin(status);
            }
            else
            {
                SetupBuyEquipButtonForPowerUp(status);
            }
        }

        private void HandleStatusChange(ItemStatus status)
        {
            if (_shopItem.Item.PowerUpType == ItemCatalogue.PowerUpType.Skin)
            {
                SetupBuyEquipButtonForSkin(status);
                _shopItem.Item.SetSkinEquipped(status == ItemStatus.Equipped);
            }
            else
            {
                SetupBuyEquipButtonForPowerUp(status);
            }
        }
        
        private void SetupBuyEquipButtonForSkin(ItemStatus status)
        {
            string buttonText = ButtonTexts[status];
            if (status == ItemStatus.Purchased)
            {
                buttonText = "Equip";
            }
            _buyEquipButtonText.text = buttonText;
            
            _buyEquipButton.interactable = status != ItemStatus.Locked;
            
            ToggleBuyEquipButtonUI(status);
        }

        private void SetupBuyEquipButtonForPowerUp(ItemStatus status)
        {
            _buyEquipButtonText.text = ButtonTexts[status];

            if (_buyEquipButton != null)
            {
                if (status != ItemStatus.Available)
                {
                    _buyEquipButton.interactable = false;
                }
                else
                {
                    _buyEquipButton.interactable = true;
                }
            }
            
            ToggleBuyEquipButtonUI(status);
        }

        private void ToggleBuyEquipButtonUI(ItemStatus status)
        {
            if (_buyEquipButtonCostItem == null)
            {
                return;
            }
            
            bool available = status == ItemStatus.Available;
            _buyEquipButtonText.gameObject.SetActive(!available);
            _buyEquipButtonCostItem.SetActive(available);
            if (available)
            {
                _buttonImage.color = _buyButtonColor;
            }
            else
            {
                _buttonImage.color = _defaultButtonColor;
            }
        }
    }
}