using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;
using System;
using static core.GameManager;
using static UnityEngine.InputSystem.InputAction;

namespace core.gameplay
{
    [Serializable]
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
        struct InputReference
        {
            public string action;
            public InputCallbackType callback_type;
            public Action<CallbackContext> callback;

            public InputReference(string action, InputCallbackType callback_type, Action<CallbackContext> callback)
            {
                this.action = action;
                this.callback_type = callback_type;
                this.callback = callback;
            }
        }

        enum InputCallbackType
        {
            Pressed,
            Released,
            Hold
        }

        // Variables
        [Header("Actor Params")]
        public bool actorUpdatesViaManager = false;

        [Header("Save System")]
        public ActorsaveData saveDataParameters = new ActorsaveData();

        // Input helpers
        private Dictionary<string, List<InputReference>> _input_callbacks = new Dictionary<string, List<InputReference>>();

        void Start()
        {
            // Register Actor
            ActOnModule((ActorManager _ref) => { _ref.RegisterActor(this); });

            // Initialize actor
            onStart();
        }

        protected virtual void onStart()
        { }

        // Called via Actor Manager Module
        public virtual void onUpdate()
        { }

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
            ClearAllRegisteredActions();
            onDestroy();
        }

        protected virtual void onDestroy()
        {
        }

        #region Helpers 

        private void AddToCallbacks(string action, InputCallbackType callback_type, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            InputReference reference = new InputReference(action, callback_type, _logicAction);

            if (!_input_callbacks.ContainsKey(_map))
            {
                _input_callbacks.Add(_map, new List<InputReference>());
            }

            _input_callbacks[_map].Add(reference);
        }

        protected void RegisterActionPressed(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed(action, _logicAction, _map);
                AddToCallbacks(action, InputCallbackType.Pressed, _logicAction, _map);
            });
        }

        protected void RegisterActionHold(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionHold(action, _logicAction, _map);
                AddToCallbacks(action, InputCallbackType.Hold, _logicAction, _map);
            });
        }

        protected void RegisterActionReleased(string action, Action<CallbackContext> _logicAction, string _map = "Player")
        {
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionReleased(action, _logicAction, _map);
                AddToCallbacks(action, InputCallbackType.Released, _logicAction, _map);
            });
        }

        protected void ClearAllRegisteredActions()
        {
            ActOnModule((InputManager _ref) =>
            {
                foreach(string map in _input_callbacks.Keys)
                {
                    foreach (InputReference input_ref in _input_callbacks[map])
                    {
                        switch (input_ref.callback_type)
                        {
                            case InputCallbackType.Pressed:
                                {
                                    _ref.UnsubscribeToActionPressed(input_ref.action, input_ref.callback, map);
                                    break;
                                }
                            case InputCallbackType.Hold:
                                {
                                    _ref.UnsubscribeToActionHold(input_ref.action, input_ref.callback, map);
                                    break;
                                }
                            case InputCallbackType.Released:
                                {
                                    _ref.UnsubscribeToActionReleased(input_ref.action, input_ref.callback, map);
                                    break;
                                }
                        }
                    }

                }
                _input_callbacks.Clear();
            });
        }
        #endregion
    }
}