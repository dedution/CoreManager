using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using core.utils;
using System.Collections.Generic;
using static core.GameManager;
using UnityEngine.InputSystem;

namespace core.modules
{
    public class MenuLayout
    {
        public List<MenuArea> menuAreas = new List<MenuArea>();
    }

    public class MenuArea
    {
        public string areaName = "";
        public Rect areaRect;
        public List<MenuProperty> areaProperties = new List<MenuProperty>();
    }

    public enum LayoutMenuProperties
    {
        Label,
        Box,
        Space,
        Toggle
    }

    public class MenuProperty
    {
        public MenuProperty(LayoutMenuProperties menuProperty, string _value)
        {
            this.menuProperty = menuProperty;
            this._value = _value;
        }

        public LayoutMenuProperties menuProperty = LayoutMenuProperties.Label;
        public string _value;
    }

    public class DebugManager : BaseModule
    {
        // TODO
        // Load layout and submenus from json file (load a default one from this package, or ignore and load one from the game logic if it exists)
        // Build the entire layout only once
        // Find a dynamic way to execute logic from the layout. Try to make this module as dynamic and reusable as possible
        // (ex. button has a string callback for "GameManager.NeedThisCall" and Debug Manager needs to parse that callback)
        // careful with callbacks for classes in inaccessible namespaces?

        /* public bool fullscreenBool = false;
        private Vector2 scrollPosition; */
        private Matrix4x4 currentMatrix;
        private bool m_isActive = false;

        private MenuLayout menuLayout;

        public bool isActive
        {
            get
            {
                return m_isActive;
            }

            set 
            {
                // Check conditions for pausing the game
                // if(value)
                m_isActive = value;

                // Wont this interfere?
                if(m_isActive)
                    SetMatrix();
                else
                    ResetMatrix();
            }
        }
        
        // Default size of debug manager
        private Vector2 nativeSize = new Vector2(1280, 720);

        public override void onInitialize()
        {
            currentMatrix = GUI.matrix;
            LoadDebugMenuStyle();

            // Attach opening menu logic to action
            ActOnModule((InputManager _ref) =>
            {
                _ref.onActionPressed("Pause", (InputAction.CallbackContext callbackContext) =>
                {
                    OpenDebug();
                });
            });
        }

        private void OpenDebug()
        {
            // Ask the gamemanager to pause the game
            isActive = !isActive;
        }

        private void LoadDebugMenuStyle()
        {
            // Loads the style json and parse it to draw the debug menu
            menuLayout = new MenuLayout();
            var newArea = new MenuArea
            {
                areaRect = new Rect(0, 0, 400, 600),
                areaName = ""
            };
            newArea.areaProperties.Add(new MenuProperty(LayoutMenuProperties.Box, "Test Box"));
            newArea.areaProperties.Add(new MenuProperty(LayoutMenuProperties.Space, "5"));
            newArea.areaProperties.Add(new MenuProperty(LayoutMenuProperties.Box, "Test Box 2"));
            newArea.areaProperties.Add(new MenuProperty(LayoutMenuProperties.Label, "Test Label"));
            menuLayout.menuAreas.Add(newArea);
        }

        private void DrawDebugMenu()
        {
            if(!isActive)
                return;

            foreach(MenuArea _area in menuLayout.menuAreas)
            {
                GUILayout.BeginArea(_area.areaRect, _area.areaName, "box");

                foreach(MenuProperty _property in _area.areaProperties)
                {
                    switch(_property.menuProperty)
                    {
                        case LayoutMenuProperties.Label:
                        {
                            GUILayout.Label(_property._value);
                            break;
                        }
                        case LayoutMenuProperties.Space:
                        {
                            GUILayout.Space(int.Parse(_property._value));
                            break;
                        }
                        case LayoutMenuProperties.Box:
                        {
                            GUILayout.Box(_property._value);
                            break;
                        }
                    }
                }

                GUILayout.EndArea();
            }
            
            /* GUILayout.BeginArea(new Rect(0, 0, 400, 600), "", "box");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(400), GUILayout.Height(500));
            GUILayout.Box("Settings:");
            GUILayout.Space(5);
            GUILayout.Box("Sound:");
            GUILayout.Label("Volume:");
            GUILayout.BeginHorizontal();
            
            AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0.0f, 1.0f);
            GUILayout.Label("" + AudioListener.volume.ToString("0.0"), "labelSound", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(500); // test scroll
            fullscreenBool = GUILayout.Toggle(fullscreenBool, "FullScreen?");
            GUILayout.EndScrollView();
            
            GUILayout.Space(25);
            GUILayout.Label("______________________________________________");
            GUILayout.Label("BUILD ID: XXXXXXX");
            GUILayout.Label("BUILD DATE: 01-01-0101");

            GUILayout.EndArea(); */
        }

        private void SetMatrix()
        {
            Vector3 scale = new Vector3 (Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, scale);
        }

        private void ResetMatrix()
        {
            GUI.matrix = currentMatrix;
        }

        public override void OnGUI()
        {
            DrawDebugMenu();
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