using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using core.modules;
using core.gameplay;
using core.utils;
using UnityEngine.Diagnostics;

namespace core
{
    public static class GameManager
    {
        // Persistent behavior that handles unity calls 
        private static CoreMonoObject coreMonoObject;
        private static ModuleManager ModuleManager = new ModuleManager();

        // Player stays on the side
        private static GameActor _playerReference;

        // TODO: Set player reference in a function
        public static GameActor m_PlayerReference
        {
            get { return _playerReference; }
            set { _playerReference = value; }
        }

        private static bool m_GamePaused = false;

        // Initialize GameManager singleton
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init()
        {
            coreMonoObject = new GameObject("MonoObject").AddComponent<CoreMonoObject>();
            UnityEngine.Object.DontDestroyOnLoad(coreMonoObject.gameObject);
            Debug.Log("Initialized Game Manager!");
            
            InputManager.Init();
            ModuleManager.Init(coreMonoObject);
            RegisterEvents();
        }

        public static void SetGamePause(bool pause_state)
        {
            m_GamePaused = pause_state;
            GameUtils.SetGameSpeed(pause_state ? 0f : 1f);
            EventManager.Trigger("GamePause", new Dictionary<string, object>{ {"current_state", m_GamePaused} });
        }

        public static bool GetGamePause()
        {
            return m_GamePaused;
        }

        private static void RegisterEvents()
        {
        }

        // TODO: DEPRECATE THIS?
        public static Coroutine RunCoroutine(IEnumerator _task)
        {
            return coreMonoObject.StartCoroutine(_task);
        }

        public static void StopCoroutine(Coroutine _task)
        {
            coreMonoObject.StopCoroutine(_task);
        }

        public static T CreateBehaviorOnDummy<T>() where T : Component
        {
            var _behavior = coreMonoObject.gameObject.AddComponent<T>();
            return _behavior;
        }

        // Safer way to use logic that interacts with modules without worrying if module even exists
        // Example how to use:
        // ActOnModule((ModuleName _ref) => {_ref.Hello();});
        public static void ActOnModule<T>(Action<T> _logic, bool forced = false, bool required = false)
        {
            if (!ModuleManager.isReady && !forced)
            {
                Logger.Error("GameManager", $"Module System [{typeof(T).Name}] is not yet initialized but something is trying to access it!");
                return;
            }

            T _module = GetLoadedModule<T>();

            if (!ReferenceEquals(_logic, null) && !ReferenceEquals(_module, null))
                _logic(_module);
            else if (required)
                Utils.ForceCrash(ForcedCrashCategory.MonoAbort);
            else
                Logger.Error("GameManager", $"Failed to access {typeof(T).Name}");
        }

        private static T GetLoadedModule<T>()
        {
            var _obj = ModuleManager.FindModule<T>();
            return (T)_obj;
        }
    }
}