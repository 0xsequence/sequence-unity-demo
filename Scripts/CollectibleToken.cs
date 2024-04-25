using MoreMountains.InfiniteRunnerEngine;
using UnityEngine;

namespace Game.Scripts
{
    public class CollectibleToken : Coin
    {
        protected override void ObjectPicked()
        {
            base.ObjectPicked();
            if (SequenceConnector.Instance == null || SequenceConnector.Instance.Wallet == null)
            {
                Debug.LogWarning("No minting will happen. Make sure SequenceConnector is in the scene and user is logged in.");
                return;
            }
            SequenceConnector.Instance.MintFungibleToken();
        }
    }
}