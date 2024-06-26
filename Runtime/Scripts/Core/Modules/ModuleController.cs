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
        private bool useJSONAutoSave = true;
        private string MODULECONFIGPATH;
        private string CONFIGFILENAME = "moduleconfig.json";

        public ModuleController()
        {
            // Load json from streaming assets
            ModulesConfiguration.Modules = new List<string>();
            MODULECONFIGPATH = Path.Combine(Application.streamingAssetsPath, "modules");

            // Create streaming assets if missing
            if (!Directory.Exists(MODULECONFIGPATH))
                Directory.CreateDirectory(MODULECONFIGPATH);

            string _modulePath = Path.Combine(MODULECONFIGPATH, CONFIGFILENAME);
            
            if(File.Exists(_modulePath))
                IOController.ReadJSONFromFile<ModuleConfig>(_modulePath, false, ProcessConfig);
        }

        public void SaveCurrentConfig()
        {
            string _modulePath = Path.Combine(MODULECONFIGPATH, CONFIGFILENAME);

            foreach (Type _type in activeModules.Keys)
                ModulesConfiguration.Modules.Add(_type.ToString());

            if (!File.Exists(_modulePath))
                IOController.WriteJSONToFile(_modulePath, ModulesConfiguration, true, false);
        }

        private void ProcessConfig(ModuleConfig _data)
        {
            ModulesConfiguration = _data;
            isConfigurationLoaded = true;
        }

        public void Init(CoreDummyObject coreDummyObject)
        {
            // Initialize modules
            foreach (BaseModule _module in InstantiateModules<BaseModule>())
            {
                activeModules.Add(_module.Module_GetType(), _module);

                // Register unity calls to delegates
                coreDummyObject.unity_GUIDelegate += _module.OnGUI;
                coreDummyObject.unity_UpdateDelegate += _module.UpdateModule;
            }

            // Initialization by order of config
            if (isConfigurationLoaded)
            {
                foreach (string _typeString in ModulesConfiguration.Modules)
                {
                    var _module = activeModules[Type.GetType(_typeString)];
                    _module.onInitialize();
                }
            }
            else if (useJSONAutoSave)
            {
                SaveCurrentConfig();
                InitAllModules();
            }
            else
                InitAllModules();

            Debug.Log(">> Modules Loaded! (" + activeModules.Count + ")");
        }

        private void InitAllModules()
        {
            // Initialize all modules loaded
            // This is only called when no default configuration is read from file
            foreach (var _module in activeModules.Values)
            {
                _module.onInitialize();
            }
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