using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.modules
{
    public class CoreMonoObject : MonoBehaviour
    {
        public delegate void ModuleUnityCallDelegate();
        public delegate void ModuleUpdateDelegate(float deltaTime, float unscaledDeltaTime);
        public ModuleUnityCallDelegate unity_GUIDelegate;
        public ModuleUpdateDelegate unity_UpdateDelegate;

        private void Start()
        {
            GameObject event_system_prefab = Resources.Load<GameObject>("EventSystem");
            GameObject event_system = Instantiate(event_system_prefab);
            event_system.transform.parent = transform;
        }

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