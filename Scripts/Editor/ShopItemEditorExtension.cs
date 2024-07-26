using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Sequence.Contracts;
using Sequence.Provider;
using Sequence.Transactions;
using Sequence.Wallet;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor
{
    [CustomEditor(typeof(ShopItem))]
    public class ShopItemEditorExtension : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ShopItem shopItem = (ShopItem)target;

            if (GUILayout.Button("Update Contract Minting Requirements"))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(RunUpdateMintingRequirements(shopItem));
            }
        }
        
        private IEnumerator RunUpdateMintingRequirements(ShopItem shopItem)
        {
            yield return UpdateMintingRequirements(shopItem);
        }
        
        private async Task UpdateMintingRequirements(ShopItem shopItem)
        {
            string privateKey = Environment.GetEnvironmentVariable("JELLY_FOREST_ADMIN_PRIVATE_KEY"); // Note: in order for this to work, you must first set the environment variable in your shell session and then run the script in ConfigureEnvironmentVariables.sh to open Unity Hub and the project
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                Debug.LogError("Private key not set in environment variables");
            }
            EOAWallet wallet = new EOAWallet(privateKey);
            
            int costItems = shopItem.CostItems.Length;
            BigInteger[] burnTokenIds = new BigInteger[costItems];
            BigInteger[] burnTokenAmounts = new BigInteger[costItems];
            for (int i = 0; i < costItems; i++)
            {
                burnTokenIds[i] = BigInteger.Parse(shopItem.CostItems[i].Item.TokenId);
                burnTokenAmounts[i] = shopItem.CostItems[i].Amount;
            }
            int requiredAchievements = shopItem.RequiredAchievements.Length;
            BigInteger[] holdTokenIds = new BigInteger[requiredAchievements];
            BigInteger[] holdTokenAmounts = new BigInteger[requiredAchievements];
            for (int i = 0; i < requiredAchievements; i++)
            {
                holdTokenIds[i] = BigInteger.Parse(shopItem.RequiredAchievements[i].TokenId);
                holdTokenAmounts[i] = 1;
            }
            
            try
            {
                BurnToMint burnToMint = new BurnToMint();
                var currentMintingRequirements = await burnToMint.GetMintingRequirements(shopItem.Item.TokenId);
                if (currentMintingRequirements.IsEqualTo(new Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]>(burnTokenIds, burnTokenAmounts, holdTokenIds, holdTokenAmounts)))
                {
                    Debug.Log($"Minting requirements for {shopItem.name} are already up to date");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            try
            {
                BurnToMint burnToMint = new BurnToMint();
                await burnToMint.SetMintingRequirements(wallet, shopItem.Item.TokenId, burnTokenIds, burnTokenAmounts, holdTokenIds, holdTokenAmounts);
                Debug.Log($"Minting requirements for {shopItem.name} updated successfully");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public static class MintingRequirementTupleExtensions
    {
        public static bool IsEqualTo(this Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]> tuple, Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]> other)
        {
            if (tuple.Item1.Length != other.Item1.Length || tuple.Item2.Length != other.Item2.Length || tuple.Item3.Length != other.Item3.Length || tuple.Item4.Length != other.Item4.Length)
            {
                return false;
            }

            for (int i = 0; i < tuple.Item1.Length; i++)
            {
                if (tuple.Item1[i] != other.Item1[i] || tuple.Item2[i] != other.Item2[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < tuple.Item3.Length; i++)
            {
                if (tuple.Item3[i] != other.Item3[i] || tuple.Item4[i] != other.Item4[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}