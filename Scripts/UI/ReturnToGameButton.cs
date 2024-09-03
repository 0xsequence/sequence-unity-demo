using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.UI
{
    public class ReturnToGameButton : MonoBehaviour
    {
        public void LoadGame()
        {
            SceneManager.LoadScene("MenuScene");
            
            gameObject.SetActive(false);
        }
    }
}