using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class ButtonSwitcher : MonoBehaviour
    {
        private GameObject[] _buttons;
        private int _activeIndex = 0;
        private int _buttonsCount;

        private void Awake()
        {
            Button[] buttons = GetComponentsInChildren<Button>();
            _buttonsCount = buttons.Length;
            _buttons = new GameObject[_buttonsCount];
            for (int i = 0; i < _buttonsCount; i++)
            {
                _buttons[i] = buttons[i].gameObject;
                _buttons[i].SetActive(false);
            }
            
            _buttons[0].SetActive(true);
        }

        public void Switch()
        {
            _buttons[_activeIndex].SetActive(false);
            _activeIndex++;
            if (_activeIndex >= _buttonsCount)
            {
                _activeIndex = 0;
            }
            _buttons[_activeIndex].SetActive(true);
        }
    }
}