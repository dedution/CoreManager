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

        private static InputManager _instance = null;
        private PlayerInput m_PlayerInput;
        private InputActionAsset m_DefaultActionAsset;
        private DefaultActionControls _DefaultActions = new DefaultActionControls();

        // Action config by platform
        private Dictionary<string, InputActionAsset> m_InputActionConfigs = new Dictionary<string, InputActionAsset>();

        // Load buttons by platform as well

        public InputManager()
        {
            if (_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {
            m_PlayerInput = GameManager.CreateBehaviorOnDummy<PlayerInput>();
            m_PlayerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            // Load default Input
            m_DefaultActionAsset = _DefaultActions.asset;
            LoadActionAssetConfiguration(m_DefaultActionAsset);

            // Try to load a default Input from Resources
            LoadActionAssetConfiguration("Input/DefaultInputAsset");

            // Load overrides?
        }

        public static void LoadActionAssetConfiguration(InputActionAsset _asset, string _currentActionMap = "Player")
        {
            // Set the action asset
            _instance.m_PlayerInput.actions = _asset;
            SwitchCurrentMap(_currentActionMap);
        }

        public static void LoadActionAssetConfiguration(string _asset, string _currentActionMap = "Player")
        {
            // Set the action asset
            GameManager.RunCoroutine(LoadActionAsset(_asset, _currentActionMap));
        }

        private static IEnumerator LoadActionAsset(string _asset, string _currentActionMap = "Player")
        {
            ResourceRequest request = Resources.LoadAsync<InputActionAsset>(_asset);
            yield return request;
            
            if(request.asset != null)
                LoadActionAssetConfiguration(request.asset as InputActionAsset, _currentActionMap);
            else
                Debug.LogWarning("[RESOURCES] Default Input Asset not found!");
        }

        public static void SwitchCurrentMap(string _map)
        {
            // Set the action asset
            _instance.m_PlayerInput.SwitchCurrentActionMap(_map);
        }

        /* button was pressed or is held */
        public static void onActionHold(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].performed += _logicAction;
        }

        public static void UnsubscribeToActionHold(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].performed -= _logicAction;
        }

        /* button was pressed */
        public static void onActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].started += _logicAction;
        }

        /* button was released */
        public static void onActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].canceled += _logicAction;
        }

        public static void UnsubscribeToActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].started -= _logicAction;
        }

        public static void UnsubscribeToActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction)
        {
            _instance.m_PlayerInput.actions[_action].canceled -= _logicAction;
        }

        public static bool IsActionPressed(string _action)
        {
            return _instance.m_PlayerInput.actions[_action].IsPressed();
        }

        public static bool IsActionReleased(string _action)
        {
            return _instance.m_PlayerInput.actions[_action].WasReleasedThisFrame();
        }

        public static bool IsActionPressedThisFrame(string _action)
        {
            return _instance.m_PlayerInput.actions[_action].WasPressedThisFrame();
        }

        // Read direct values from action
        public static T ReadActionValue<T>(string _action) where T : struct
        {
            return _instance.m_PlayerInput.actions[_action].ReadValue<T>();
        }

        // Here for now
        public static void LimitGamepadToFirst()
        {
            _instance.m_PlayerInput.currentActionMap.devices = new[] { Gamepad.all[0] };
        }
    }
}