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
                    currentInputMap = InputManager.GetCurrentMap();
                    InputManager.SwitchCurrentMap("Debug");

                    previous_cursorLockMode = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    InputManager.SwitchCurrentMap(currentInputMap);
                    Cursor.lockState = previous_cursorLockMode;
                }
            }
        }

        public override void onInitialize()
        {
            // Attach opening menu logic to action map of Player
            InputManager.onActionPressed("Debug", (InputAction.CallbackContext callbackContext) =>
                {
                    ToggleDebugMenu(true);
                });

            InputManager.onActionPressed("Debug", (InputAction.CallbackContext callbackContext) =>
            {
                ToggleDebugMenu(false);
            }, "Debug");
        }

        // Load menu only after the scene loads
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SpawnMenu()
        {
            GameObject prefab = Resources.Load<GameObject>("Scenes/ConsoleCanvas");
            if (prefab == null)
            {
                Debug.LogError($"[PackageBootstrap] Prefab not found at Resources/Scenes/ConsoleCanvas");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(instance);
        }

        private void ToggleDebugMenu(bool state)
        {
            isActive = state;
            EventManager.Trigger("Console", new Dictionary<string, object> { { "isMenuOpen", state } });
        }
    }
}