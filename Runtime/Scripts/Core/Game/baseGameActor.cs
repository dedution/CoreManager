using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;
using System;
using static core.GameManager;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace core.gameplay
{
    [System.Serializable]
    public struct ActorsaveData
    {
        public bool Enabled;
        public string GUID;
        public void GenerateGUID()
        {
            if (string.IsNullOrWhiteSpace(GUID))
            {
                GUID = System.Guid.NewGuid().ToString();
            }
        }
    }

    public abstract class baseGameActor : MonoBehaviour
    {
        // Variables
        [Header("Actor Params")]
        public bool actorUpdatesViaManager = false;

        [Header("Save System")]
        public ActorsaveData saveDataParameters = new ActorsaveData();
        
        // Input helpers 
        private Dictionary<string, Action<CallbackContext>> _input_pressed_callbacks = new Dictionary<string, Action<CallbackContext>>();
        private Dictionary<string, Action<CallbackContext>> _input_released_callbacks = new Dictionary<string, Action<CallbackContext>>();
        private Dictionary<string, Action<CallbackContext>> _input_hold_callbacks = new Dictionary<string, Action<CallbackContext>>();
        private List<string> _input_map_names = new List<string>();

        void Start()
        {
            // Register Actor
            ActOnModule((ActorManager _ref) => {_ref.RegisterActor(this);});

            // Initialize actor
            onStart();
        }

        protected virtual void onStart()
        {}

        // Called via Actor Manager Module
        public virtual void onUpdate()
        {}

        protected T SaveSystem_GetData<T>(string _dataKey, T _defaultData)
        {
            T _data = _defaultData;

            if (saveDataParameters.Enabled)
                ActOnModule((SaveSystemManager _ref) => { _data = _ref.SaveSystem_GameData_Get(saveDataParameters.GUID, _dataKey, _defaultData); }, true);

            return _data;
        }

        protected void SaveSystem_SetData<T>(string _dataKey, T _savedata)
        {
            if (saveDataParameters.Enabled)
                ActOnModule((SaveSystemManager _ref) => { _ref.SaveSystem_GameData_Set(saveDataParameters.GUID, _dataKey, _savedata); }, true);
        }

        private void OnDestroy()
        {
            // Unregister Actor
            ActOnModule((ActorManager _ref) => { _ref.UnregisterActor(this); });

            foreach (string map in _input_map_names)
            {
                ClearAllRegisteredActions(map);
            }

            _input_map_names.Clear();
            
            onDestroy();
        }

        protected virtual void onDestroy()
        {
        }

        #region Helpers 
        protected void RegisterActionPressed(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            RegisterInputMap(_map);
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed(action, _logicAction, _map);
                _input_pressed_callbacks.Add(action, _logicAction);
            });
        }

        protected void RegisterActionHold(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            RegisterInputMap(_map);
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionHold(action, _logicAction, _map);
                _input_released_callbacks.Add(action, _logicAction);
            });
        }

        protected void RegisterActionReleased(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            RegisterInputMap(_map);
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionReleased(action, _logicAction, _map);
                _input_released_callbacks.Add(action, _logicAction);
            });
        }

        protected void ClearAllRegisteredActions(string _map = "Player")
        {
            ActOnModule((InputManager _ref) =>
            {
                foreach (string action in _input_pressed_callbacks.Keys)
                {
                    _ref.UnsubscribeToActionPressed(action, _input_released_callbacks[action], _map);
                }
                foreach (string action in _input_hold_callbacks.Keys)
                {
                    _ref.UnsubscribeToActionHold(action, _input_released_callbacks[action], _map);
                }
                foreach (string action in _input_released_callbacks.Keys)
                {
                    _ref.UnsubscribeToActionReleased(action, _input_released_callbacks[action], _map);
                }

                _input_released_callbacks.Clear();
            });
        }

        protected void RegisterInputMap(string map)
        {
            if (!_input_map_names.Contains(map))
            {
                _input_map_names.Add(map);
            }
        }

        #endregion
    }
}