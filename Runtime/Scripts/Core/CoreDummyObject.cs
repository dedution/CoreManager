using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core
{
    public class CoreDummyObject : MonoBehaviour
    {
        public delegate void ModuleUnityCallDelegate();
        public ModuleUnityCallDelegate unity_GUIDelegate;

        private void OnGUI()
        {
            if(!ReferenceEquals(unity_GUIDelegate, null))
                unity_GUIDelegate();
        }
    }
}