using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using core.modules;
using UnityEngine;

namespace core
{
    public class ModuleController
    {
        /** Module loader and controller
        * Some of these modules have direct instance access via semi-singleton implementation to facilitate frequent calls
        */

        private List<BaseModule> activeModules = new List<BaseModule>();

        public ModuleController()
        {
        }

        public void Init(CoreDummyObject coreDummyObject)
        {
            // Initialize modules - uses dummy object namespace to find other classes in the same namepsace and inits them
            foreach (BaseModule _module in InstantiateModules<BaseModule>(coreDummyObject))
            {
                activeModules.Add(_module);

                //Register unity calls to delegates
                coreDummyObject.unity_GUIDelegate += _module.OnGUI;
                coreDummyObject.unity_UpdateDelegate += _module.UpdateModule;

                _module.MonoObject = coreDummyObject;
            }

            // Initialize modules -- prevents load order problems.
            activeModules.ForEach(delegate(BaseModule _module) {_module.onInitialize();});

            Debug.Log("# Modules Loaded! (" + activeModules.Count + ")");
        }
        public T FindModule<T>()
        {
            var _obj = activeModules.OfType<T>().ElementAt(0);
            if(_obj == null) Debug.LogError("Couldn't find loaded module: " + typeof(T));

            return (T)_obj;
        }

        private List<Type> SearchTypeInNamespace(object instance)
        {
            string ns = instance.GetType().Namespace;
            Type instanceType = instance.GetType();
            List<Type> results = instance.GetType().Assembly.GetTypes().Where(tt => tt.Namespace == ns &&
                                                                              tt != instanceType).ToList();
            return results;
        }

        private List<T> InstantiateModules<T>(object instance)
        {
            List<T> instances = new List<T>();

            foreach (Type t in SearchTypeInNamespace(instance))
            {
                if (t.IsSubclassOf(typeof(T)))
                {
                    T i = (T)Activator.CreateInstance(t);
                    instances.Add(i);
                }
            }

            return instances;
        }
    }
}