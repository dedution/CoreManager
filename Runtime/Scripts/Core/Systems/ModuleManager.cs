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
    public class ModuleManager
    {
        // Configuration JSON
        [System.Serializable]
        public struct ModuleConfig
        {
            public List<string> Modules;
        }

        /// <summary>
        /// Module loader and manager
        /// </summary>
        
        // TODO: Allow debug builds to load the config json from the streaming assets folder

        private Dictionary<Type, BaseModule> activeModules = new Dictionary<Type, BaseModule>();
        private ModuleConfig ModulesConfiguration = new ModuleConfig();
        private bool isConfigurationLoaded = false;
        public bool isReady = false;
        private const string CONFIGFILENAME = "module_config.json";
        private const string CONFIGRESOURCEPATH = "modules/module_config";
        private const string CONFIGRESOURCESFOLDER = "Resources/modules";

        public ModuleManager()
        {
            ModulesConfiguration.Modules = new List<string>();

            TextAsset _config = Resources.Load<TextAsset>(CONFIGRESOURCEPATH);
            if (_config != null)
                ProcessConfig(JsonUtility.FromJson<ModuleConfig>(_config.text));
        }

        public void SaveCurrentConfig()
        {
            ModulesConfiguration.Modules.Clear();

            foreach (Type _type in activeModules.Keys)
                ModulesConfiguration.Modules.Add(_type.ToString());

            isConfigurationLoaded = true;

#if UNITY_EDITOR
            string _modulePath = GetModuleConfigAssetPath();
            Directory.CreateDirectory(Path.GetDirectoryName(_modulePath));
            File.WriteAllText(_modulePath, JsonUtility.ToJson(ModulesConfiguration));
            UnityEditor.AssetDatabase.Refresh();
#else
            Logger.Warn("Module config can only be saved to Resources while running in the Unity Editor.", GetType().Name);
#endif
        }

#if UNITY_EDITOR
        private string GetModuleConfigAssetPath()
        {
            return Path.Combine(Application.dataPath, CONFIGRESOURCESFOLDER, CONFIGFILENAME);
        }
#endif

        private void ProcessConfig(ModuleConfig _data)
        {
            ModulesConfiguration = _data;
            if (ModulesConfiguration.Modules == null)
                ModulesConfiguration.Modules = new List<string>();

            isConfigurationLoaded = true;
        }

        public void Init(CoreMonoObject coreMonoObject)
        {
            Logger.Info("Initialized Module Controller!", GetType().Name);

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
            Logger.Info("Modules Loaded! (" + activeModules.Count + ")", GetType().Name);
        }

        public T FindModule<T>()
        {
            var _type = typeof(T);
            var _obj = activeModules[_type];

            if (ReferenceEquals(_obj, null))
            {
                Logger.Error("Couldn't find loaded module: " + _type, GetType().Name);
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
