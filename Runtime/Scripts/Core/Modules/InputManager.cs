using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace core.modules
{
    public class InputManager : BaseModule
    {
        /* private delegate void ButtonDelegate();
        private Dictionary<string, Action> m_ButtonDelegates = new Dictionary<string, Action>(); */

        private static InputManager _instance = null;

        public InputManager() {
            if(_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {
            // Load Input configuration and populate dictionary of button actions 
            // m_ButtonDelegates.Add("", delegate() {});
        }

        /* public override void UpdateModule()
        {

        }

        // TODO
        // Dynamic handling of inputs for controllers and prompt icon packs
        // Loading configurations from external resources, allowing for complete sets of inputs without touching the codebase
        // m_ButtonDelegates.Add("Jump", delegate() { onJump(); });

        public static void BindToButton(string _buttonID, Action _bindAction)
        {
            if(_instance.m_ButtonDelegates.ContainsKey(_buttonID) && _instance.m_ButtonDelegates[_buttonID] != null)
                _instance.m_ButtonDelegates[_buttonID] += _bindAction;

            Debug.LogWarningFormat("Failed to bind action to button! [{}]", _buttonID);
        }

        public static void UnbindToButton(string _buttonID, Action _bindAction)
        {
            if(_instance.m_ButtonDelegates.ContainsKey(_buttonID) && _instance.m_ButtonDelegates[_buttonID] != null)
                _instance.m_ButtonDelegates[_buttonID] -= _bindAction;
            
            Debug.LogWarningFormat("Failed to unbind action to button! [{}]", _buttonID);
        } */
    } 
}