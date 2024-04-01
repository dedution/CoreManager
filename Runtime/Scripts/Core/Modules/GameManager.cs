using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using core.modules;

namespace core
{
    public class GameManager
    {
        private static GameManager _instance = null;

        //Persistent behavior that handles unity calls 
        private CoreDummyObject coreDummyObject;
        private ModuleController moduleController = new ModuleController();
        private bool m_gameManagerWasInit = false;

        private GameManager()
        {
            //Initialize mono dummy gameobject
            coreDummyObject = new GameObject("MonoObject").AddComponent<CoreDummyObject>();
            UnityEngine.Object.DontDestroyOnLoad(coreDummyObject.gameObject);
        }

        // Avoid access to the instance
        private static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameManager();

                return _instance;
            }
        }

        // Initialize
        public static void Init()
        {
            if (!Instance.m_gameManagerWasInit)
            {
                Instance.moduleController.Init(Instance.coreDummyObject);
                Instance.m_gameManagerWasInit = true;
            }
        }

        public static Coroutine RunCoroutine(IEnumerator _task)
        {
            return Instance.coreDummyObject.StartCoroutine(_task);
        }

        public static T CreateBehaviorOnDummy<T>() where T : Component
        {
            var _behavior = Instance.coreDummyObject.gameObject.AddComponent<T>();
            return _behavior;
        }

        // Easier direct access to module reference
        private static T GetLoadedModule<T>()
        {
            var _obj = Instance.moduleController.FindModule<T>();
            return (T)_obj;
        }

        // Safer way to use logic that interacts with modules without worrying if module even exists
        // Example how to use:
        // ActOnModule((ModuleName _ref) => {_ref.Hello();});
        public static void ActOnModule<T>(Action<T> _logic)
        {
            T _module = GetLoadedModule<T>();

            if(!ReferenceEquals(_logic, null) && !ReferenceEquals(_module, null))
                _logic(_module);
        }

        // Returns the state of pause
        public static bool Game_SetPauseState(bool state)
        {
            // Can we pause the game?
            // Do logic for freezing and unfreezing time
            // Trigger pause event for whoever is listening

            return Game_GetPauseState();
        }

        public static bool Game_GetPauseState()
        {
            return false;
        }
    }
}