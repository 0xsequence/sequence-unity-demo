using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class AchievementUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        private Item _achievement;
        
        public void Assemble(Item achievement)
        {
            _achievement = achievement;
            
            _icon.sprite = achievement.Icon;
            // Todo implement
        }
    }
}