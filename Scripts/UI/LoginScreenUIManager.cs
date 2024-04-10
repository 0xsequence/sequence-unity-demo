using System;
using Sequence.Demo;
using UnityEngine;

namespace Game.Scripts
{
    public class LoginScreenUIManager : MonoBehaviour
    {
        private void Start()
        {
            LoginPanel loginPanel = GetComponentInChildren<LoginPanel>();
            if (loginPanel == null)
            {
                Debug.LogError("LoginPanel not found!");
            }

            loginPanel.Open();
        }
    }
}