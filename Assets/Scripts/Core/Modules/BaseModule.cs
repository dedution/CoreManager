
using UnityEngine;

namespace core.modules
{
    public abstract class BaseModule
    {
        public BaseModule()
        {
            onInitialize();
        }

        protected virtual void onInitialize()
        {
            Debug.Log("Initialization method from module is not configured ( " + GetType().ToString() + ")");
        }
    }
}