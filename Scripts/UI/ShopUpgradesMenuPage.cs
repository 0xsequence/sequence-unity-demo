using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Sequence.Contracts;
using Sequence.Demo;
using Sequence.Provider;
using Sequence.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Game.Scripts
{
    public class ShopUpgradesMenuPage : ShopPage
    {
        [SerializeField] private VerticalLayoutGroup _shopLayoutGroup;
        [SerializeField] protected GameObject _shopItemPrefab;
        [SerializeField] private Color _defaultButtonColor;
        [SerializeField] private Color _buyButtonColor;
        [SerializeField] private ScrollRect _scrollView;
        [SerializeField] private GameObject _divider;

        private RectTransform _shopLayoutGroupRectTransform;
        private ShopItemUI[] _shopItemUIs;
        private List<GameObject> _dividers = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            _shopLayoutGroupRectTransform = _shopLayoutGroup.GetComponent<RectTransform>();
        }
        
        protected override void PopulateShop()
        {
            DestroyExistingShopItemUIs();
            int items = _shopItems.Length;
            _shopItemUIs = new ShopItemUI[items];
            for (int i = 0; i < items; i++)
            {
                ShopItem shopItem = _shopItems[i];
                _shopItemsById[shopItem.Item.TokenId] = shopItem;
                GameObject shopItemGameObject = Instantiate(_shopItemPrefab, _shopLayoutGroupRectTransform);
                ShopItemUI shopItemUI = shopItemGameObject.GetComponent<ShopItemUI>();
                _shopItemUIs[i] = shopItemUI;
                shopItemUI.Assemble(shopItem, _inventory);
                if (i < items - 1)
                {
                    GameObject divider = Instantiate(_divider, _shopLayoutGroupRectTransform);
                    _dividers.Add(divider);
                }
            }
            Invoke(nameof(UpdateScrollView), 0.1f); // Sometimes Instantiate takes too long to complete and we don't yet have children in the layout group
        }

        private void DestroyExistingShopItemUIs()
        {
            if (_shopItemUIs == null)
            {
                return;
            }
            
            int shopItems = _shopItemUIs.Length;
            for (int i = 0; i < shopItems; i++)
            {
                Destroy(_shopItemUIs[i].gameObject);
            }
            _shopItemUIs = null;
            
            int dividers = _dividers.Count;
            for (int i = 0; i < dividers; i++)
            {
                Destroy(_dividers[i]);
            }
            _dividers = new List<GameObject>();
        }

        private void UpdateScrollView()
        {
            float contentHeight = _shopLayoutGroup.preferredHeight;
            _shopLayoutGroupRectTransform.sizeDelta =
                new Vector2(_shopLayoutGroupRectTransform.sizeDelta.x, contentHeight);
            
            Invoke(nameof(SetScrollViewToTop), .2f);
        }

        private void SetScrollViewToTop()
        {
            _scrollView.verticalScrollbar.value = 1;
        }

        protected override void RefreshShopItemUIs()
        {
            int shopItems = _shopItems.Length;
            for (int i = 0; i < shopItems; i++)
            {
                _shopItemUIs[i].Assemble(_shopItems[i], _inventory);
            }
        }
    }
}