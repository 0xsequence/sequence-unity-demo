using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class CostItemUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _checkmark;
        
        private CostItem _costItem;

        public void Assemble(CostItem costItem, ItemStatus status)
        {
            _costItem = costItem;
            _icon.sprite = costItem.Item.Icon;
            if (status == ItemStatus.Locked)
            {
                _checkmark.SetActive(costItem.CanAfford);
            }
        }
    }
}