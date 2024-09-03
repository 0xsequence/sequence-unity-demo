using Sequence.Demo;

namespace Game.Scripts.UI
{
    public class SignedInSuccessfullyPopup : UIPage
    {
        public override void Open(params object[] args)
        {
            _gameObject.SetActive(true);
            _animator.AnimateIn( _openAnimationDurationInSeconds);
            Invoke(nameof(Close), 5f);
        }
    }
}
