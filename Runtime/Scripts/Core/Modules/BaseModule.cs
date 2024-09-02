
using System;
using UnityEngine;

namespace core.modules
{
    public abstract class BaseModule
    {
        // For debugging information
        private Matrix4x4 currentMatrix;
        protected Vector2 nativeSize = new Vector2(1280, 720);

        public BaseModule()
        {
            currentMatrix = GUI.matrix;
        }

        public Type Module_GetType()
        {
            return GetType();
        }

        public virtual void onInitialize()
        {
        }

        public virtual void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {   
        }

        public virtual void OnGUI()
        {
        }

        public void Hello()
        {
            // Test function for debugging
        }

        protected void SetUIMatrix()
        {
            Vector3 scale = new Vector3 (Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, scale);
        }

        protected void ResetUIMatrix()
        {
            GUI.matrix = currentMatrix;
        }
    }
}