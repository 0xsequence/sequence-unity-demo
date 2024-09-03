using System;
using Sequence.Demo;

namespace Game.Scripts.UI
{
    public class ItemPurchasedPopup : UIPage
    {
        private void Start()
        {
            SequenceConnector.Instance.OnItemPurchasedSuccessfully += ItemPurchased;
        }

        private void ItemPurchased()
        {
            Open();
        }

        public override void Open(params object[] args)
        {
            _gameObject.SetActive(true);
            _animator.AnimateIn( _openAnimationDurationInSeconds);
            Invoke(nameof(Close), 5f);
        }

        private void OnDestroy()
        {
            SequenceConnector.Instance.OnItemPurchasedSuccessfully -= ItemPurchased;
        }
    }
}
