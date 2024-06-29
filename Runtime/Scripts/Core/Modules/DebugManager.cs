using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using core.utils;
using System.Collections.Generic;
using static core.GameManager;
using UnityEngine.InputSystem;
using System.Linq;
using core.debug.layouts;
using static core.debug.diagnostics.DebugDiagnostics;
using UnityEditor;

namespace core.modules
{
    public class DebugManager : BaseModule
    {
        // TODO
        // Load layout and submenus from json file (load a default one from this package, or ignore and load one from the game logic if it exists)
        // Separate classes and interfaces into a new source file and namespace
        // Slides and toggle needs an updating function that is called every time the menu is opened. this update needs to come from a delegate 

        private bool m_isActive = false;

        private Rect m_rectDiagnosticsLabel = new Rect(10, 35, 150, 20);

        // Things like FPS counter, memory usage and batching
        private bool showDiagnostics = false;

        public bool m_showDiagnostics
        {
            get
            {
                return showDiagnostics;
            }

            set 
            {
                showDiagnostics = value;

                if(showDiagnostics)
                    StartTrackingAssetMemory();
                else
                    StopTrackingAssetMemory();
            }
        }

        private MenuLayout menuLayout;

        private int propertySelectionIndex = 0;

        private bool navIsPressed = false;

        private string currentInputMap = "";

        public bool isActive
        {
            get
            {
                return m_isActive;
            }

            set 
            {
                m_isActive = value;

                if(m_isActive)
                {
                    propertySelectionIndex = 0;

                    ActOnModule((InputManager _ref) =>
                    {
                        currentInputMap = _ref.GetCurrentMap();
                        _ref.SwitchCurrentMap("UI");
                    }, true);
                }
                else
                {
                    ActOnModule((InputManager _ref) =>
                    {
                        _ref.SwitchCurrentMap(currentInputMap);
                    }, true);
                }
            }
        }
        
        private static GUIStyle m_labelStyle = new GUIStyle();

        public override void onInitialize()
        {
            LoadDebugMenuStyle();

            // Attach opening menu logic to action map of Player
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed("Pause", (InputAction.CallbackContext callbackContext) =>
                {
                    ToggleDebugMenu();
                });
            }, true);
            
            // Attach opening menu logic to action map of UI
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed("Pause", (InputAction.CallbackContext callbackContext) =>
                {
                    ToggleDebugMenu();
                }, "UI");
            }, true);

            // Initialize the diagnostics logic
            InitDiagnostics();
        }

        private void ToggleDebugMenu()
        {
            // Ask the gamemanager to pause the game
            // If the game is already paused or it isnt possible, skip this
            isActive = !isActive;
        }

        private void LoadDebugMenuStyle()
        {
            // Loads the style json and parse it to draw the debug menu
            menuLayout = new MenuLayout();
            List<MenuProperty> _areaProperties = new List<MenuProperty>
            {
                new MenuProperty_Space(30),
                new MenuProperty_Box("Settings"),

                new MenuProperty_Label("Option Number one!", true),
                new MenuProperty_Space(10),
                new MenuProperty_Box("Sound"),
                new MenuProperty_Label("Option Number two!", true),
                new MenuProperty_Space(50),
                new MenuProperty_Label("______________________________________________", Color.red),
                new MenuProperty_Space(5),
                new MenuProperty_Label("BUILD ID: XXXXXXX", Color.red),
                new MenuProperty_Label("BUILD DATE: 01-01-0101", Color.red)
            };

            var newArea = new MenuArea
            {
                areaRect = new Rect(25, 25, 400, 600),
                areaName = "- DEBUG MENU -",
                areaProperties = _areaProperties
            };

            menuLayout.menuAreas.Add(newArea);
        }

        private void UpdateControls(int maxSelectables)
        {
            ActOnModule((InputManager _ref) =>
            {
                Vector2 _nav = _ref.ReadActionValue("Navigate", false, "UI", Vector2.zero);

                if(_nav.y < 0f && !navIsPressed)
                {
                    if(propertySelectionIndex >= maxSelectables - 1)
                        propertySelectionIndex = 0;
                    else
                        propertySelectionIndex ++;

                    navIsPressed = true;
                }
                else if(_nav.y > 0f && !navIsPressed)
                {
                    if(propertySelectionIndex == 0)
                        propertySelectionIndex = maxSelectables - 1;
                    else
                        propertySelectionIndex--;

                    navIsPressed = true;
                }
                else if(_nav.y == 0f)
                    navIsPressed = false;
            });
        }

        private void DrawDebugMenu()
        {
            if(!isActive)
                return;

            foreach(MenuArea _area in menuLayout.menuAreas)
            {
                GUILayout.BeginArea(_area.areaRect, _area.areaName, "box");
                
                int counter = 0;

                foreach(MenuProperty _property in _area.areaProperties)
                {
                    _property.RunProperty();

                    if(_property.isSelectable) {
                        if(counter == propertySelectionIndex)
                            _property.ShowSelector();
                        else
                            _property.ClearSelector();

                        counter++;
                    }
                }

                UpdateControls(counter);

                GUILayout.EndArea();
            }
        }

        private void DrawDiagnostics()
        {
            if(!m_showDiagnostics)
                return;
            
            DrawFramerate();

            DrawMemoryUsage();
        }

        private void DrawFramerate()
        {
            // Draw the current framerate at the top left of the screen
            m_labelStyle.alignment = TextAnchor.MiddleCenter;
            float currentFramerate = GetCurrentFramerate();

            if(currentFramerate < 30)
                m_labelStyle.normal.textColor = Color.red;
            else if(currentFramerate < 50)
                m_labelStyle.normal.textColor = Color.yellow;
            else
                m_labelStyle.normal.textColor = Color.green;

            GUI.Label(new Rect(10, 10, 50, 20), string.Format("{0} FPS", currentFramerate.ToString()), m_labelStyle);
        }

        private void DrawMemoryUsage()
        {
            m_labelStyle.alignment = TextAnchor.MiddleLeft;
            m_labelStyle.normal.textColor = Color.white;
            m_rectDiagnosticsLabel.x = nativeSize.x - 240;
            m_rectDiagnosticsLabel.y = 35;

            DrawDiagnosticLabel("--- {0} --------------", "MEMORY USAGE", 25);
            DrawDiagnosticLabel("Total Reserved memory: {0} MB", GetTotalReservedMemory().ToString(), 15);
            DrawDiagnosticLabel("Allocated memory: {0} MB", GetAllocatedMemory().ToString(), 15);
            DrawDiagnosticLabel("Reserved but not allocated: {0} MB", GetReservedMemory().ToString(), 15);
            DrawDiagnosticLabel("Used Mesh Memory: {0} MB", GetUsedMeshMemory().ToString(), 15);
            DrawDiagnosticLabel("Used Texture Memory: {0} MB", GetUsedTextureMemory().ToString(), 25);

            DrawDiagnosticLabel("--- {0} --------------", "RENDERING", 25);
            DrawDiagnosticLabel("Draw Calls: {0}", GetCurrentDrawCalls().ToString(), 15);
            DrawDiagnosticLabel("Shadow Casters: {0}", GetCurrentShadowCasters().ToString(), 15);
            DrawDiagnosticLabel("Triangle Count: {0}", GetCurrentTriangles().ToString(), 25);

            DrawDiagnosticLabel("--- {0} --------------", "RENDERING", 25);
            DrawDiagnosticLabel("Draw Calls: {0}", GetCurrentDrawCalls().ToString(), 15);
            DrawDiagnosticLabel("Shadow Casters: {0}", GetCurrentShadowCasters().ToString(), 15);
            DrawDiagnosticLabel("Triangle Count: {0}", GetCurrentTriangles().ToString(), 15);
        }

        private void DrawDiagnosticLabel(string labelFormat, string labelValue, int incremental)
        {
            GUI.Label(m_rectDiagnosticsLabel, string.Format(labelFormat, labelValue), m_labelStyle);
            m_rectDiagnosticsLabel.y += incremental;
        }

        public override void UpdateModule()
        {
            UpdateDiagnostics();
        }

        public override void OnGUI()
        {
            SetUIMatrix();

            DrawDiagnostics();

            DrawDebugMenu();

            ResetUIMatrix();
        }

        private void ExecuteExternalCallBack(string _command)
        {
            // Use: (ex.) GameUtils.TestFunction
            // Class name needs to be part of modules namespace (maybe isolate it into a separate namespace, used by the debug manager only)
            // Method to call needs to be static (separate these functions into a utils class)
            // Update this logic to include custom parameters if necessary
            
            var cmd = _command.Split('.');

            Type type = Type.GetType(cmd[0]);
            MethodInfo method = type.GetMethod(cmd[1]);
            method.Invoke(null, null);
        }
    }
}