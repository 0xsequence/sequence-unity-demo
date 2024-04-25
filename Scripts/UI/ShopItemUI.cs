using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText;
        [SerializeField] private Transform _costItemContainer;
        [SerializeField] private TextMeshProUGUI _buyEquipButtonText;
        [SerializeField] private GameObject _costItemPrefab;
        [SerializeField] private GameObject _shopLoadingScreenPrefab;
        [SerializeField] private GameObject _buyEquipButtonCostItem;
        [SerializeField] private Color _defaultButtonColor;
        [SerializeField] private Color _buyButtonColor;
        [SerializeField] private GameObject _componentsRegion;
        [SerializeField] private TextMeshProUGUI _tokenCostText;
        
        private Inventory _inventory;
        private ShopItem _shopItem;
        private Button _buyEquipButton;
        private BuyEquipButton _buyEquip;

        public void Assemble(ShopItem item, Inventory inventory)
        {
            _shopItem = item;
            _inventory = inventory;

            _icon.sprite = item.Item.Icon;
            _itemNameText.text = item.Item.Name;
            _itemDescriptionText.text = item.Item.Description;
            
            _buyEquipButton = _buyEquipButtonText.GetComponentInParent<Button>();

            SetupCostItems(item);
            
            _buyEquip = new BuyEquipButton(_shopItem, _shopLoadingScreenPrefab, transform.parent.parent.parent.parent, _buyEquipButtonText, _buyEquipButton, item.Status, _buyEquipButtonCostItem, _defaultButtonColor, _buyButtonColor);
        }

        private void SetupCostItems(ShopItem item)
        {
            DestroyAnyExistingCostItemUIs();
            
            int costItems = item.CostItems.Length;
            if (costItems == 1 && item.CostItems[0].Item.TokenId == SequenceConnector.CollectibleTokenId)
            {
                _componentsRegion.SetActive(false);
                _tokenCostText.text = item.CostItems[0].Amount.ToString();
                return;
            }
            
            for (int i = 0; i < costItems; i++)
            {
                CostItem costItem = item.CostItems[i];
                if (costItem.Item.TokenId == SequenceConnector.CollectibleTokenId)
                {
                    _tokenCostText.text = costItem.Amount.ToString();
                    continue;
                }
                GameObject costItemGameObject = Instantiate(_costItemPrefab, _costItemContainer);
                CostItemUI costItemUI = costItemGameObject.GetComponent<CostItemUI>();
                costItemUI.Assemble(costItem, item.Status);
            }
        }

        private void DestroyAnyExistingCostItemUIs()
        {
            CostItemUI[] costItems = _costItemContainer.GetComponentsInChildren<CostItemUI>();
            int costItemsLength = costItems.Length;
            for (int i = 0; i < costItemsLength; i++)
            {
                Destroy(costItems[i].gameObject);
            }
        }
        
        public void Buy()
        {
            _buyEquip.Buy();
        }
    }
}