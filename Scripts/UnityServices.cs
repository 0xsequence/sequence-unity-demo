using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Game.Scripts
{
#if UNITY_IOS || UNITY_ANDROID
    public class UnityServices : MonoBehaviour
    {
        public string environment = "production";

        private async void Start()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(environment);
                await Unity.Services.Core.UnityServices.InitializeAsync(options);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }


            UnityIAP.GetInstance(); // This will initialize IAP
        }
    }
#endif
}
