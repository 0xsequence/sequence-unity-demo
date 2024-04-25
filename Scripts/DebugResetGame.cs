using UnityEngine;

namespace Game.Scripts
{
    public class DebugResetGame : MonoBehaviour
    {
        [SerializeField] private int _clicksRequired = 5;
        [SerializeField] private float _clicksResetTime = .5f;
        private int _clicks = 0;
        private float _lastClickTime = 0f;
    
        private void Update()
        {
            if (Time.time - _lastClickTime > _clicksResetTime)
            {
                _clicks = 0;
            }
        }
    
        public void ResetGameButtonClicked()
        {
            _clicks++;
            _lastClickTime = Time.time;
            if (_clicks >= _clicksRequired)
            {
                Debug.Log("Resetting game");
                _clicks = 0;
                SequenceConnector.Instance.Inventory.ResetSave();
            }
        }
    }
}
