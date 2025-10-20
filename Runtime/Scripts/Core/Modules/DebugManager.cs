using UnityEngine;
using System.Collections.Generic;
using static core.GameManager;
using UnityEngine.InputSystem;

namespace core.modules
{
    public class DebugManager : BaseModule
    {
        private bool m_isActive = false;
        private CursorLockMode previous_cursorLockMode = CursorLockMode.None;

        private string currentInputMap = "";

        public bool isActive
        {
            get
            {
                return m_isActive;
            }
            set
            {
                if (m_isActive == value)
                    return;
                    
                m_isActive = value;

                if (m_isActive)
                {
                    ActOnModule((InputManager _ref) =>
                    {
                        currentInputMap = _ref.GetCurrentMap();
                        _ref.SwitchCurrentMap("UI");
                    }, true);

                    previous_cursorLockMode = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    ActOnModule((InputManager _ref) =>
                    {
                        _ref.SwitchCurrentMap(currentInputMap);
                    }, true);

                    Cursor.lockState = previous_cursorLockMode;
                }
            }
        }

        public override void onInitialize()
        {
            // Attach opening menu logic to action map of Player
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed("Debug", (InputAction.CallbackContext callbackContext) =>
                {
                    ToggleDebugMenu(true);
                });

                _ref.onActionPressed("Debug", (InputAction.CallbackContext callbackContext) =>
                {
                    ToggleDebugMenu(false);
                }, "UI");
            }, true);
        }

        // Load on first scene ready!
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SpawnMenu()
        {
            GameObject prefab = Resources.Load<GameObject>("ConsoleCanvas");
            if (prefab == null)
            {
                Debug.LogError($"[PackageBootstrap] Prefab not found at Resources/ConsoleCanvas");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(instance);
        }

        private void ToggleDebugMenu(bool state)
        {
            isActive = state;

            ActOnModule((EventManager _ref) =>
            {
                _ref.TriggerEvent("Console", new Dictionary<string, object> { { "isMenuOpen", state } });
            });
        }
    }
}