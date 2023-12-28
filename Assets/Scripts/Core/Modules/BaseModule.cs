
using UnityEngine;

namespace core.modules
{
    public abstract class BaseModule
    {
        public BaseModule()
        {
            //Module initialization logic
            onInitialize();
        }

        protected virtual void onInitialize()
        {
            Debug.Log("Initialization method from module is not configured ( " + GetType().ToString() + ")");
        }

        public virtual void OnGUI()
        {
        }
    }
}