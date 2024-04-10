using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "CostItem", menuName = "Sequence/Game/CostItem")]
    public class CostItem : ScriptableObject
    {
        public int Amount;
        public Item Item;

        public bool CanAfford { get; private set; } = false;
        
        public void SetCanAfford(bool canAfford)
        {
            CanAfford = canAfford;
        }

        public static CostItem Create(Item item, int amount)
        {
            CostItem newCostItem = CreateInstance<CostItem>();
            newCostItem.Item = item;
            newCostItem.Amount = amount;
            newCostItem.CanAfford = false;
            return newCostItem;
        }
    }
}