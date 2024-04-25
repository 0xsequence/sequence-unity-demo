using System;
using Sequence.Demo;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class InGameTokenDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tokensAmountText;
        
        private Inventory _inventory;
        private TextSetter _tokensText;
        
        private void Start()
        {
            if (SequenceConnector.Instance == null)
            {
                Debug.LogError("Unable to display tokens as SequenceConnector is not found. User likely has not logged in.");
                return;
            }

            _inventory = SequenceConnector.Instance.Inventory;
            _inventory.OnInventoryBalanceChanged += HandleInventoryBalanceChanged;
            _tokensText = new TextSetter(_tokensAmountText);
            _tokensText.SetText(_inventory.GetTokens().ToString("### ### ##0").TrimStart(' '), true);
        }

        private void OnDestroy()
        {
            _inventory.OnInventoryBalanceChanged -= HandleInventoryBalanceChanged;
        }

        private void HandleInventoryBalanceChanged(InventoryBalanceChanged balanceChanged)
        {
            if (balanceChanged.TokenId == SequenceConnector.CollectibleTokenId)
            {
                _tokensText.SetText(balanceChanged.NewAmount.ToString("### ### ##0").TrimStart(' '), true);
            }
        }
    }
}