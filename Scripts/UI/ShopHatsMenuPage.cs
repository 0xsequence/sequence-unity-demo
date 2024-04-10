using System.Collections.Generic;
using Sequence.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class ShopHatsMenuPage : ShopPage
    {
        [SerializeField] private TextMeshProUGUI _buyEquipButtonText;
        [SerializeField] private Image _hatImage;
        [SerializeField] private Image _playerImage;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private GameObject _shopLoadingScreenPrefab;
        [SerializeField] private GameObject _costItemObject;
        [SerializeField] private Color _defaultButtonColor;
        [SerializeField] private Color _buyButtonColor;

        private List<ShopItem> _hats;
        private Button _buyEquipButton;
        private BuyEquipButton _buyEquip;
        private int _currentHatIndex = 0;
        private Item _greenSkin;

        protected override void Awake()
        {
            base.Awake();
            _hats = new List<ShopItem>();
            int shopItems = _shopItems.Length;
            if (shopItems == 0)
            {
                Debug.LogError("No shop items found in ShopHatsMenuPage.");
            }
            for (int i = 0; i < shopItems; i++)
            {
                if (_shopItems[i].Item.IsHat)
                {
                    _hats.Add(_shopItems[i]);
                }
            }

            if (shopItems != _hats.Count)
            {
                Debug.LogError("You've added a non-hat item to the hats shop. This item will not be sold at the hat shop.");
            }
            
            _buyEquipButton = _buyEquipButtonText.GetComponentInParent<Button>();
        }

        public override void Open(params object[] args)
        {
            base.Open(args);
            Item.OnHatEquipped += UnequipAllOtherHats;
        }
        
        public override void Close()
        {
            Item.OnHatEquipped -= UnequipAllOtherHats;
            base.Close();
        }

        protected override void PopulateShop()
        {
            RefreshShopItemUIs();
        }

        protected override void RefreshShopItemUIs()
        {
            if (IsPlayerGreen())
            {
                _playerImage.color = Color.green;
            }
            else
            {
                _playerImage.color = Color.white;
            }
            
            ShopItem hat = _hats[_currentHatIndex];
            _hatImage.sprite = hat.Item.Icon;
            _buyEquip = new BuyEquipButton(hat, _shopLoadingScreenPrefab, transform, _buyEquipButtonText, _buyEquipButton, hat.Status, _costItemObject, _defaultButtonColor, _buyButtonColor);
            _costText.text = hat.CostItems[0].Amount.ToString();
        }

        private bool IsPlayerGreen()
        {
            if (_greenSkin == null)
            {
                _greenSkin = _itemCatalogue.ItemsByPowerUpType[ItemCatalogue.PowerUpType.Skin].Find(item => item.Name == "Green Dude");
            }

            return _greenSkin.IsSkinEquipped();
        }

        private void UnequipAllOtherHats(string tokenId)
        {
            int hats = _hats.Count;
            for (int i = 0; i < hats; i++)
            {
                if (_hats[i].Item.TokenId != tokenId && _hats[i].Status == ItemStatus.Equipped)
                {
                    _hats[i].SetStatus(ItemStatus.Purchased);
                }
            }
        }

        public void NextItem()
        {
            _currentHatIndex++;
            if (_currentHatIndex >= _hats.Count)
            {
                _currentHatIndex = 0;
            }
            PopulateShop();
        }
        
        public void PreviousItem()
        {
            _currentHatIndex--;
            if (_currentHatIndex < 0)
            {
                _currentHatIndex = _hats.Count - 1;
            }
            PopulateShop();
        }
        
        public void Buy()
        {
            _buyEquip.Buy();
        }
    }
}