using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using core.modules;
using UnityEngine;
using static core.GameManager;

namespace core
{
    public class ModuleController
    {
        // Configuration JSON
        [System.Serializable]
        public struct ModuleConfig
        {
            public List<string> Modules;
        }

        /// <summary>
        /// Module loader and controller
        /// </summary>
        private Dictionary<Type, BaseModule> activeModules = new Dictionary<Type, BaseModule>();
        private ModuleConfig ModulesConfiguration = new ModuleConfig();
        private bool isConfigurationLoaded = false;
        public bool isReady = false;
        private string MODULECONFIGPATH;
        private string CONFIGFILENAME = "moduleconfig.json";

        // TODO: Rework moduleconfig.json loading and saving logic to use the resources folder instead
        public ModuleController()
        {
            // Load json from streaming assets
            ModulesConfiguration.Modules = new List<string>();

            // Never expose this to the streaming assets folder. Prevent tampering using the Resources folder
            MODULECONFIGPATH = Path.Combine(Application.streamingAssetsPath, "modules");

            // Create streaming assets if missing
            if (!Directory.Exists(MODULECONFIGPATH))
            {
                Directory.CreateDirectory(MODULECONFIGPATH);
            }

            string _modulePath = Path.Combine(MODULECONFIGPATH, CONFIGFILENAME);

            if (File.Exists(_modulePath))
                IOController.ReadJSONFromFile<ModuleConfig>(_modulePath, false, ProcessConfig);
        }

        public void SaveCurrentConfig()
        {
            string _modulePath = Path.Combine(MODULECONFIGPATH, CONFIGFILENAME);

            foreach (Type _type in activeModules.Keys)
                ModulesConfiguration.Modules.Add(_type.ToString());

            if (!File.Exists(_modulePath))
                IOController.WriteJSONToFile(_modulePath, ModulesConfiguration, true, false);

            isConfigurationLoaded = true;
        }

        private void ProcessConfig(ModuleConfig _data)
        {
            ModulesConfiguration = _data;
            isConfigurationLoaded = true;
        }

        public void Init(CoreMonoObject coreMonoObject)
        {
            Debug.Log("Initialized Module Controller!");

            // Instantiate modules
            foreach (BaseModule _module in InstantiateModules<BaseModule>())
            {
                activeModules.Add(_module.Module_GetType(), _module);
            }

            // Save current config is it wasnt before
            if (!isConfigurationLoaded)
            {
                SaveCurrentConfig();
            }

            // Initialize active modules
            foreach (string _typeString in ModulesConfiguration.Modules)
            {
                var _module = activeModules[Type.GetType(_typeString)];
                coreMonoObject.unity_GUIDelegate += _module.OnGUI;
                coreMonoObject.unity_UpdateDelegate += _module.UpdateModule;
                _module.onInitialize();
            }

            isReady = true;
            Debug.Log(">> Modules Loaded! (" + activeModules.Count + ")");
        }

        public T FindModule<T>()
        {
            var _type = typeof(T);
            var _obj = activeModules[_type];

            if (ReferenceEquals(_obj, null))
            {
                Debug.LogError("Couldn't find loaded module: " + _type);
                return default(T);
            }

            return (T)Convert.ChangeType(_obj, _type);
        }

        private List<Type> SearchTypeInNamespace<T>()
        {
            string ns = typeof(T).Namespace;
            List<Type> results = new List<Type>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                results.AddRange(a.GetTypes().Where(tt => tt.Namespace == ns && tt != typeof(T)).ToList());
            }

            return results;
        }

        private List<T> InstantiateModules<T>()
        {
            List<T> instances = new List<T>();

            foreach (Type moduleType in SearchTypeInNamespace<T>())
            {
                if (moduleType.IsSubclassOf(typeof(T)))
                {
                    if (isConfigurationLoaded && !ModulesConfiguration.Modules.Contains(moduleType.ToString()))
                        continue;

                    T i = (T)Activator.CreateInstance(moduleType);
                    instances.Add(i);
                }
            }

            return instances;
        }
    }
}