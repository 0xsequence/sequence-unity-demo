using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Game.Scripts
{
    public class ItemCatalogue
    {
        public List<Item> Items = new List<Item>();
        public Dictionary<PowerUpType, List<Item>> ItemsByPowerUpType = new Dictionary<PowerUpType, List<Item>>();
        public Dictionary<string, Item> ItemsByTokenId = new Dictionary<string, Item>();

        public enum PowerUpType
        {
            Boost,
            DoubleJump,
            QuickDrop,
            ExtraLives,
            None,
            Skin
        }

        public ItemCatalogue()
        {
            Object[] loadedItems = Resources.LoadAll("ScriptableObjects/Items", typeof(Item));
            int items = loadedItems.Length;
            for (int i = 0; i < items; i++)
            {
                Item item = (Item) loadedItems[i];
                Items.Add(item);
                if (!ItemsByPowerUpType.ContainsKey(item.PowerUpType))
                {
                    ItemsByPowerUpType[item.PowerUpType] = new List<Item>();
                }
                ItemsByPowerUpType[item.PowerUpType].Add(item);
                
                ItemsByTokenId[item.TokenId] = item;
            }
        }
    }
}