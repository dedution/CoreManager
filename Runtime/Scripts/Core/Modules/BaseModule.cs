
using System;
using UnityEngine;

namespace core.modules
{
    public abstract class BaseModule
    {
        public BaseModule()
        {
        }

        public Type Module_GetType()
        {
            return this.GetType();
        }

        public virtual void onInitialize()
        {
        }

        public virtual void UpdateModule()
        {   
        }

        public virtual void OnGUI()
        {
        }

        public void Hello()
        {
            // Test function for debugging
        }
    }
}