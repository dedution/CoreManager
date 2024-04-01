
using System;
using UnityEngine;

namespace core.modules
{
    public abstract class BaseModule
    {
        public CoreDummyObject MonoObject { get; set; }

        public BaseModule()
        {
        }

        public Type Module_GetType()
        {
            return this.GetType();
        }

        public virtual void onInitialize()
        {
            //Debug.Log("Initialization method from module is not configured ( " + GetType().ToString() + ")");
        }

        public virtual void UpdateModule()
        {
            
        }

        public virtual void OnGUI()
        {
        }

        public void Hello()
        {
            
        }
    }
}