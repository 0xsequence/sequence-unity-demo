using System;
using System.Collections.Generic;
using MoreMountains.InfiniteRunnerEngine;
using MoreMountains.Tools;
using Sequence.Utils;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(MMSimpleObjectPooler))]
    [RequireComponent(typeof(CharacterSkins))]
    public class JumperWithPowerUps : Jumper
    {
        [SerializeField] private float _quickDropGravityScalingFactor = 3f;
        [SerializeField] private float _quickDropExtraRewardSpeedThreshhold = -45f;
        private Dictionary<ItemCatalogue.PowerUpType, uint> _powerUpTiers = new Dictionary<ItemCatalogue.PowerUpType, uint>();
        private Inventory _inventory;
        private bool _isQuickDropping = false;
        private Rigidbody2D _rigidbody;
        private MMSimpleObjectPooler _tokenRewardObjectPooler;
        private float _xDistanceBetweenTokenRewards = 5.5f;
        private uint _quickDropTier;
        private SpriteRenderer _spriteRenderer;
        private CharacterSkins _characterSkins;
        private Color _defaultColor;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody2D>();
            _tokenRewardObjectPooler = GetComponent<MMSimpleObjectPooler>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _characterSkins = GetComponent<CharacterSkins>();
        }

        protected override void Start()
        {
            base.Start();
            _inventory = SequenceConnector.Instance.Inventory;
            if (_inventory != null)
            {
                _inventory.OnInventoryBalanceChanged += HandleInventoryBalanceChanged;
            }
            else
            {
                Debug.LogError("Inventory not found. User likely has not logged in.");
            }
            UpdatePowerUpTiers();
            ApplySkins();
            _defaultColor = _spriteRenderer.color;
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryBalanceChanged -= HandleInventoryBalanceChanged;
            }
        }

        private void HandleInventoryBalanceChanged(InventoryBalanceChanged inventoryBalanceChanged)
        {
            if (inventoryBalanceChanged.TokenId == SequenceConnector.CollectibleTokenId)
            {
                return;
            }
            UpdatePowerUpTiers();
        }
        
        private void UpdatePowerUpTiers()
        {
#if UNITY_EDITOR
            _powerUpTiers[ItemCatalogue.PowerUpType.DoubleJump] = 1;
            _powerUpTiers[ItemCatalogue.PowerUpType.QuickDrop] = 3;
            _powerUpTiers[ItemCatalogue.PowerUpType.ExtraLives] = 0;
            _powerUpTiers[ItemCatalogue.PowerUpType.Boost] = 0;
#else
            List<ItemCatalogue.PowerUpType> powerUpTypes =
                EnumExtensions.GetEnumValuesAsList<ItemCatalogue.PowerUpType>();
            powerUpTypes.Remove(ItemCatalogue.PowerUpType.None);
            powerUpTypes.Remove(ItemCatalogue.PowerUpType.Skin);
            int powerUpTypesCount = powerUpTypes.Count;
            for (int i = 0; i < powerUpTypesCount; i++)
            {
                ItemCatalogue.PowerUpType powerUpType = powerUpTypes[i];
                _powerUpTiers[powerUpType] = _inventory.GetBestTieredItem(powerUpType);
            }
#endif
            
            NumberOfJumpsAllowed = (int) _powerUpTiers[ItemCatalogue.PowerUpType.DoubleJump] + 1;
        }

        protected override void Update()
        {
            base.Update();
            
            if (Input.touches.Length > 1)
            {
                InitiateQuickDrop();
            }
            
#if UNITY_STANDALONE_OSX || UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
            {
                InitiateQuickDrop();
            }
#endif
            
            if (_grounded)
            {
                StopQuickDrop();
            }

            if (_isQuickDropping)
            {
                AdjustColor();
            }
        }

        private void InitiateQuickDrop()
        {
            if (_isQuickDropping || _grounded)
            {
                return;
            }
            
            if (_powerUpTiers[ItemCatalogue.PowerUpType.QuickDrop] < 1)
            {
                return;
            }
            
            _isQuickDropping = true;

            _rigidbody.gravityScale *= _quickDropGravityScalingFactor;
        }

        private void StopQuickDrop()
        {
            if (!_isQuickDropping)
            {
                return;
            }
            
            _isQuickDropping = false;

            _rigidbody.gravityScale /= _quickDropGravityScalingFactor;

            SpawnTokenRewards(_powerUpTiers[ItemCatalogue.PowerUpType.QuickDrop]);
            ResetColor();
        }

        private void SpawnTokenRewards(uint tier)
        {
            int toSpawn = CalculateTokenRewardsToSpawn(tier);
            Vector3 position = transform.position;
            for (int i = 0; i < toSpawn; i++)
            {
                GameObject tokenReward = _tokenRewardObjectPooler.GetPooledGameObject();
                tokenReward.transform.position = position + new Vector3(_xDistanceBetweenTokenRewards * i, 0, 0);
                tokenReward.SetActive(true);
            }
        }

        private int CalculateTokenRewardsToSpawn(uint tier)
        {
            if (tier <= 1)
            {
                return 0;
            }

            if (tier == 2)
            {
                return 1;
            }

            if (_rigidbody.velocity.y < _quickDropExtraRewardSpeedThreshhold)
            {
                return 2;
            }

            return 1;
        }
        
        private void AdjustColor()
        {
            if (_powerUpTiers[ItemCatalogue.PowerUpType.QuickDrop] < 3)
            {
                return;
            }

            float percentageOfMaxSpeed = Math.Min(1f, _rigidbody.velocity.y / _quickDropExtraRewardSpeedThreshhold);
            Color color = _spriteRenderer.color;
            color = Color.Lerp(color, Color.red, percentageOfMaxSpeed);
            _spriteRenderer.color = color;
        }

        private void ResetColor()
        {
            _spriteRenderer.color = _defaultColor;
        }
        
        private void ApplySkins()
        {
            if (_inventory == null)
            {
                return;
            }
            
            
            List<Item> allSkins = SequenceConnector.Instance.ItemCatalogue.ItemsByPowerUpType[ItemCatalogue.PowerUpType.Skin];
            int skinsCount = allSkins.Count;
            List<Item> equippedSkins = new List<Item>();
            for (int i = 0; i < skinsCount; i++)
            {
                if (allSkins[i].IsSkinEquipped())
                {
                    equippedSkins.Add(allSkins[i]);
                }
            }

            Item[] skins = equippedSkins.SortByTokenId();
            skinsCount = skins.Length;
            for (int i = 0; i < skinsCount; i++)
            {
                _characterSkins.ApplySkin(skins[i]);
            }
        }
    }
}