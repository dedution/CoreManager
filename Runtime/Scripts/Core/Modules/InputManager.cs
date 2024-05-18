using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

namespace core.modules
{
    public class InputManager : BaseModule
    {
        // TODO
        // Handle switch between action maps (ex: for player and for UI)
        // Handle load of json configurations for different platforms
        // Use a default map inside this package with the default binds\
        // Handle dynamic reconfiguration and loading over default

        private PlayerInput m_PlayerInput;
        private InputActionAsset m_DefaultActionAsset;
        private DefaultActionControls _DefaultActions = new DefaultActionControls();

        // Action config by platform
        // private Dictionary<string, InputActionAsset> m_InputActionConfigs = new Dictionary<string, InputActionAsset>();

        // Load buttons by platform as well

        public bool isBusyLoading = true;

        public InputManager()
        {
            m_PlayerInput = GameManager.CreateBehaviorOnDummy<PlayerInput>();
            m_PlayerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            // Load default Input
            m_DefaultActionAsset = _DefaultActions.asset;
            LoadActionAssetConfiguration(m_DefaultActionAsset);
            isBusyLoading = false;

            // Try to load a default Input from Resources
            // Dont use async cause of other modules dependencies
            LoadActionAssetConfiguration("Input/DefaultInputAsset", "Player");

            // Load overrides?
        }

        public override void onInitialize()
        {
            
        }

        public string GetCurrentScheme()
        {
            return m_PlayerInput.currentControlScheme;
        }

        public bool IsUsingGamepad()
        {
            return m_PlayerInput.currentControlScheme == "Gamepad";
        }

        public void LoadActionAssetConfiguration(InputActionAsset _asset, string _currentActionMap = "Player")
        {
            // Set the action asset
            m_PlayerInput.actions = _asset;
            SwitchCurrentMap(_currentActionMap);
        }

        public void LoadActionAssetConfiguration(string _asset, string _currentActionMap = "Player", bool useAsync = false)
        {
            isBusyLoading = true;

            // Set the action asset
            if(useAsync)
                GameManager.RunCoroutine(LoadActionAsset(_asset, _currentActionMap));
            else
            {
                InputActionAsset _inputdata = Resources.Load<InputActionAsset>(_asset);

                if(_inputdata != null)
                    LoadActionAssetConfiguration(_inputdata, _currentActionMap);
                else
                    Debug.LogWarning("[RESOURCES] Default Input Asset not found!");

                isBusyLoading = false;
            }
        }

        private IEnumerator LoadActionAsset(string _asset, string _currentActionMap = "Player")
        {
            ResourceRequest request = Resources.LoadAsync<InputActionAsset>(_asset);
            yield return request;
            
            if(request.asset != null)
                LoadActionAssetConfiguration(request.asset as InputActionAsset, _currentActionMap);
            else
                Debug.LogWarning("[RESOURCES] Default Input Asset not found!");

            isBusyLoading = false;
        }

        public void SwitchCurrentMap(string _map)
        {
            // Set the action asset
            m_PlayerInput.SwitchCurrentActionMap(_map);
        }

        public string GetCurrentMap()
        {
            return m_PlayerInput.currentActionMap.name;
        }

        public void RestoreCurrentMap()
        {
            SwitchCurrentMap(m_PlayerInput.defaultActionMap);
        }

        /* button was pressed or is held */
        public void onActionHold(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].performed += _logicAction;
        }

        public void UnsubscribeToActionHold(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].performed -= _logicAction;
        }

        /* button was pressed */
        public void onActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].started += _logicAction;
        }

        /* button was released */
        public void onActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].canceled += _logicAction;
        }

        public void UnsubscribeToActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].started -= _logicAction;
        }

        public void UnsubscribeToActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            m_PlayerInput.actions.FindActionMap(_map)[_action].canceled -= _logicAction;
        }

        public bool IsActionPressed(string _action, string _map = "Player")
        {
            return m_PlayerInput.actions.FindActionMap(_map)[_action].IsPressed();
        }

        public bool IsActionReleased(string _action, string _map = "Player")
        {
            return m_PlayerInput.actions.FindActionMap(_map)[_action].WasReleasedThisFrame();
        }

        public bool IsActionPressedThisFrame(string _action, string _map = "Player")
        {
            return m_PlayerInput.actions.FindActionMap(_map)[_action].WasPressedThisFrame();
        }

        // Read direct values from action
        public T ReadActionValue<T>(string _action, bool checkReleased = false, string _map = "Player", T _default = default(T)) where T : struct
        {
            InputAction _InputAction = m_PlayerInput.actions.FindActionMap(_map)[_action];

            if(!checkReleased)
                return _InputAction.ReadValue<T>();
            else
                return _InputAction.WasReleasedThisFrame() ? _default : _InputAction.ReadValue<T>();
        }

        // Here for now
        public void LimitGamepadToFirst()
        {
            m_PlayerInput.currentActionMap.devices = new[] { Gamepad.all[0] };
        }
    }
}