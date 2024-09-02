using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.modules
{
    // ONLY LOGIC RELATED TO MODULES HERE
    // Update gui and normal game ticks
    public class CoreDummyObject : MonoBehaviour
    {
        public delegate void ModuleUnityCallDelegate();
        public delegate void ModuleUpdateDelegate(float deltaTime, float unscaledDeltaTime);
        public ModuleUnityCallDelegate unity_GUIDelegate;
        public ModuleUpdateDelegate unity_UpdateDelegate;

        private void OnGUI()
        {
            if(!ReferenceEquals(unity_GUIDelegate, null))
                unity_GUIDelegate();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // Only update modules through here
            if(!ReferenceEquals(unity_UpdateDelegate, null))
                unity_UpdateDelegate(deltaTime, unscaledDeltaTime);
        }
    }
}