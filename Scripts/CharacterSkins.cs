using System;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSkins : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _hatSpriteRenderer;
        
        private string _greenSkinTokenId;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            Item greenSkin = Resources.Load<Item>("ScriptableObjects/Items/GreenSkin");
            _greenSkinTokenId = greenSkin.TokenId;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ApplySkin(Item skin)
        {
            if (skin.TokenId == _greenSkinTokenId)
            {
                _spriteRenderer.color = Color.green;
            }
            
            if (skin.IsHat)
            {
                _hatSpriteRenderer.sprite = skin.Icon;
            }
        }
    }
}