using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "Item", menuName = "Sequence/Game/Item")]
    public class Item : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Icon;
        public string TokenId;
        public ItemCatalogue.PowerUpType PowerUpType;
        public uint Tier;
        public bool IsHat = false;

        private bool _skinEquipped;
        private bool _skinEquippedCheckedThisSession = false;

        public static Action<string> OnHatEquipped;

        public bool IsSkinEquipped()
        {
            if (PowerUpType != ItemCatalogue.PowerUpType.Skin)
            {
                Debug.LogError("IsSkinEquipped called on non-skin item. You've made an error somewhere.");
                return false;
            }
            
            if (PlayerPrefs.HasKey($"{SequenceConnector.Instance.Wallet.GetWalletAddress()}SkinEquipped{TokenId}"))
            {
                _skinEquipped = PlayerPrefs.GetInt($"{SequenceConnector.Instance.Wallet.GetWalletAddress()}SkinEquipped{TokenId}") == 1;
            }
            else
            {
                _skinEquipped = false;
            }
            return _skinEquipped;
        }
        
        public void SetSkinEquipped(bool equipped)
        {
            if (PowerUpType != ItemCatalogue.PowerUpType.Skin)
            {
                Debug.LogError("SetSkinEquipped called on non-skin item. You've made an error somewhere.");
                return;
            }
            
            if (IsHat && equipped)
            {
                OnHatEquipped?.Invoke(TokenId);
            }
            
            _skinEquipped = equipped;
            PlayerPrefs.SetInt($"{SequenceConnector.Instance.Wallet.GetWalletAddress()}SkinEquipped{TokenId}", equipped ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static class ItemExtensions
    {
        public static Item[] SortByTokenId(this Item[] items)
        {
            return items.OrderBy(item => item.TokenId).ToArray();
        }
        
        public static Item[] SortByTokenId(this List<Item> items)
        {
            return items.OrderBy(item => item.TokenId).ToArray();
        }
    }
}