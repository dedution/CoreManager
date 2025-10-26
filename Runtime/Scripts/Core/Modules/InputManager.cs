using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using Codice.Client.BaseCommands;

namespace core.modules
{
    public class InputManager : BaseModule
    {
        private PlayerInput m_PlayerInput;
        private InputActionAsset m_GameActionAsset;

        public bool isBusyLoading = true;

        public InputManager()
        {
            m_PlayerInput = GameManager.CreateBehaviorOnDummy<PlayerInput>();
            m_PlayerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            m_GameActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();

            InputActionAsset default_actions = LoadActionAssetConfiguration("Input/DefaultInputAsset");
            InputActionAsset game_actions = LoadActionAssetConfiguration("Input/InputAsset");

            m_GameActionAsset = game_actions == null ? default_actions : game_actions;

            SetAssetConfiguration(m_GameActionAsset);
        }
        
        private void SetAssetConfiguration(InputActionAsset _asset, string _currentActionMap = "Player")
        {
            if (m_PlayerInput == null)
                return;

            // Set the action asset
            m_PlayerInput.actions = _asset;
            m_PlayerInput.actions.Enable();

            // Refresh maps otherwise weird behavior may happen on first input
            foreach (InputActionMap map in m_PlayerInput.actions.actionMaps)
            {
                map.Enable();
                map.Disable();
            }

            SwitchCurrentMap(_currentActionMap);
        }

        public override void onInitialize()
        {

        }

        public string GetCurrentScheme()
        {
            if (m_PlayerInput == null)
                return "";

            return m_PlayerInput.currentControlScheme;
        }

        public bool IsUsingGamepad()
        {
            if (m_PlayerInput == null)
                return false;

            return m_PlayerInput.currentControlScheme == "Gamepad";
        }

        public InputActionAsset LoadActionAssetConfiguration(string _asset, string _currentActionMap = "Player", bool useAsync = false)
        {
            InputActionAsset _inputdata = Resources.Load<InputActionAsset>(_asset);

            if (_inputdata != null)
                return _inputdata;
            else
                Debug.LogWarning($"[RESOURCES] Input Asset {_asset} not found!");
            return null;
        }

        public void SwitchCurrentMap(string _map)
        {
            if (m_PlayerInput == null)
                return;

            // Set the action asset
            m_PlayerInput.SwitchCurrentActionMap(_map);
        }

        public string GetCurrentMap()
        {
            if (m_PlayerInput == null)
                return "";

            return m_PlayerInput.currentActionMap.name;
        }

        public void RestoreCurrentMap()
        {
            if (m_PlayerInput == null)
                return;

            SwitchCurrentMap(m_PlayerInput.defaultActionMap);
        }

        /* button was pressed or is held */
        public void onActionHold(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].performed += _logicAction;
        }

        public void UnsubscribeToActionHold(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].performed -= _logicAction;
        }

        /* button was pressed */
        public void onActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].started += _logicAction;
        }

        /* button was released */
        public void onActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].canceled += _logicAction;
        }

        public void UnsubscribeToActionPressed(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].started -= _logicAction;
        }

        public void UnsubscribeToActionReleased(string _action, Action<InputAction.CallbackContext> _logicAction, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                m_PlayerInput.actions.FindActionMap(_map)[_action].canceled -= _logicAction;
        }

        public bool IsActionPressed(string _action, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return false;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                return m_PlayerInput.actions.FindActionMap(_map)[_action].IsPressed();
            else
                return false;
        }

        public bool IsActionReleased(string _action, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return false;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                return m_PlayerInput.actions.FindActionMap(_map)[_action].WasReleasedThisFrame();
            else
                return false;
        }

        public bool IsActionPressedThisFrame(string _action, string _map = "Player")
        {
            if (m_PlayerInput == null)
                return false;

            if (m_PlayerInput.actions.FindActionMap(_map).FindAction(_action) != null)
                return m_PlayerInput.actions.FindActionMap(_map)[_action].WasPressedThisFrame();
            else
                return false;
        }

        // Read direct values from action
        public T ReadActionValue<T>(string _action, bool checkReleased = false, string _map = "Player", T _default = default(T)) where T : struct
        {
            if (m_PlayerInput == null)
                return default;

            InputAction _InputAction = m_PlayerInput.actions.FindActionMap(_map)[_action];

            if (_InputAction != null && !checkReleased)
                return _InputAction.ReadValue<T>();
            else
                return (_InputAction == null || _InputAction.WasReleasedThisFrame()) ? _default : _InputAction.ReadValue<T>();
        }

        // Here for now
        public void LimitGamepadToFirst()
        {
            if (m_PlayerInput == null)
                return;

            m_PlayerInput.currentActionMap.devices = new[] { Gamepad.all[0] };
        }
    }
}