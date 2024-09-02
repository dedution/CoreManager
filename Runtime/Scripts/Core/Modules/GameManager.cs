using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using core.modules;
using core.gameplay;

namespace core
{
    public class GameManager
    {
        private static GameManager _instance = null;

        //Persistent behavior that handles unity calls 
        private CoreDummyObject coreDummyObject;
        private ModuleController moduleController = new ModuleController();
        private bool m_gameManagerWasInit = false;

        // Player stays on the side
        private static baseGameActor _playerReference;
        public static baseGameActor m_PlayerReference
        {
            get { return _playerReference; }
            set { _playerReference = value; }
        }

        private static bool m_GamePaused = false;
        
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
        public static void ActOnModule<T>(Action<T> _logic, bool forced = false)
        {
            if (!Instance.moduleController.isReady && !forced)
            {
                Debug.LogError("Module System [" + typeof(T).Name + "] is not yet initialized but something is trying to access it!");
                return;
            }

            T _module = GetLoadedModule<T>();

            if (!ReferenceEquals(_logic, null) && !ReferenceEquals(_module, null))
                _logic(_module);
        }

        public static bool Game_CanPause()
        {
            return true;
        }
        
        // Returns the state of pause
        public static void Game_SetPauseState(bool state)
        {
            if(m_GamePaused == state || !Game_CanPause())
                return;

            // When can we pause the game?
            // Trigger event announcing the pause state change 
            ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("gamePause", new Dictionary<string, object> { { "pause", state } });
            });

            Game_SetGameSpeed(state ? 0f : 1f);

            m_GamePaused = state;
        }

        public static bool Game_GetPauseState()
        {
            return m_GamePaused;
        }
        
        public static void Game_SetGameSpeed(float _targetSpeed)
        {
            Time.timeScale = _targetSpeed;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }

        public static float Game_GetGameSpeed()
        {
            return Time.timeScale;
        }
    }
}