using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using core.utils;
using System.Collections.Generic;
using static core.GameManager;
using UnityEngine.InputSystem;
using System.Linq;

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

    public class MenuProperty_ScrollView : MenuProperty
    {
        private bool isStart = false;
        private GUILayoutOption gUILayoutOption1;
        private GUILayoutOption gUILayoutOption2;
        private Vector2 scrollPosition;

        bool MenuProperty.isSelectable { get { return false; }}

        public MenuProperty_ScrollView(bool isStart, GUILayoutOption gUILayoutOption1, GUILayoutOption gUILayoutOption2)
        {
            this.isStart = isStart;
            this.gUILayoutOption1 = gUILayoutOption1;
            this.gUILayoutOption2 = gUILayoutOption2;
        }

        public MenuProperty_ScrollView(bool isStart)
        {
            this.isStart = isStart;
        }

        public void RunProperty()
        {
            if(isStart)
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, gUILayoutOption1, gUILayoutOption2);
            else
                GUILayout.EndScrollView();
        }
    }

    public class MenuProperty_Box : MenuProperty
    {
        string _data = "";
        bool MenuProperty.isSelectable { get { return false; }}
        
        public MenuProperty_Box(string _customData)
        {
            _data = _customData;
        }

        public void RunProperty()
        {
            GUILayout.Box(_data);
        }
    }

    public class MenuProperty_Horizontal : MenuProperty
    {
        bool isStart = false;
        bool MenuProperty.isSelectable { get { return false; }}
        
        public MenuProperty_Horizontal(bool isStart)
        {
            this.isStart = isStart;
        }

        public void RunProperty()
        {
            if(isStart)
                GUILayout.BeginHorizontal();
            else
                GUILayout.EndHorizontal();
        }
    }

    public class MenuProperty_Toggle: MenuProperty
    {
        bool val = false;
        string label = "";

        public Action<bool> OnUpdate;
        bool MenuProperty.isSelectable { get { return false; }}
        
        public MenuProperty_Toggle(bool val, string label, Action<bool> OnUpdate)
        {
            this.val = val;
            this.label = label;
            this.OnUpdate = OnUpdate;
        }

        public void RunProperty()
        {
            val = GUILayout.Toggle(val, label);
            OnUpdate.Invoke(val);
        }
    }

    public class MenuProperty_Space : MenuProperty
    {
        float _data = 0f;
        bool MenuProperty.isSelectable { get { return false; }}
        
        public MenuProperty_Space(float _customData)
        {
            _data = _customData;
        }

        public void RunProperty()
        {
            GUILayout.Space(_data);
        }
    }

    public class MenuProperty_Label : MenuProperty
    {
        string _data = "";

        string _selector = "";

        string _style = "";

        float timer = 0f;
        bool timerDir = false;

        Color color = Color.white;
        bool MenuProperty.isSelectable { get { return true; }}

        public GUILayoutOption GUILayoutOption { get; }

        public MenuProperty_Label(string _customData)
        {
            _data = _customData;
        }

        public MenuProperty_Label(string _customData, Color color)
        {
            _data = _customData;
            
            if(color != null)
                this.color = color;
        }

        public MenuProperty_Label(string _customData, GUILayoutOption gUILayoutOption, Color color, string style = "") : this(_customData)
        {
            _data = _customData;
            GUILayoutOption = gUILayoutOption;
            _style = style;

            if(color != null)
                this.color = color;
        }

        public void RunProperty()
        {
            GUIStyle _style = new GUIStyle();
            _style.wordWrap = false;
            _style.normal.textColor = color;

            GUIStyle _styleSelector = new GUIStyle();
            _styleSelector.wordWrap = false;
            _styleSelector.normal.textColor = Color.yellow;
            _styleSelector.fontStyle = FontStyle.Bold;

            GUILayout.BeginHorizontal();
            {
                string finaldata = (_selector.Length > 0 ? "         " : "") + _data;
                GUILayout.Label(finaldata, _style, GUILayout.ExpandWidth(false));
                Rect _rect = GUILayoutUtility.GetLastRect();
                _rect.xMin += timer * 10f;

                GUI.Label(_rect, _selector, _styleSelector);
            }
            
            GUILayout.EndHorizontal();
        }

        public void ShowSelector()
        {
            _selector = ">>";

            if(timerDir)
                timer -= Time.deltaTime * 2f;
            else
                timer += Time.deltaTime * 2f;

            if(timer > 1f)
            {
                timerDir = true;
            }
            else if(timer < 0f)
            {
                timerDir = false;
            }
        }

        public void ClearSelector()
        {
            _selector = "";
        }
    }

    public class MenuProperty_Slider : MenuProperty
    {
        public float currentValue { get; set;}
        public float minValue { get; }
        public float maxValue { get; }
        bool MenuProperty.isSelectable { get { return false; }}

        public Action<float> OnUpdate;

        public MenuProperty_Slider(float currentValue, float minValue, float maxValue, Action<float> OnUpdate)
        {
            this.currentValue = currentValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.OnUpdate = OnUpdate;
        }

        public void RunProperty()
        {
            currentValue = GUILayout.HorizontalSlider(currentValue, minValue, maxValue);
            OnUpdate.Invoke(currentValue);
        }
    }

    public interface MenuProperty
    {
        bool isSelectable { get;}

        public void RunProperty()
        {
            // Run logic 
        }

        public void ShowSelector()
        {

        }

        public void ClearSelector()
        {

        }
    }

    public class DebugManager : BaseModule
    {
        // TODO
        // Load layout and submenus from json file (load a default one from this package, or ignore and load one from the game logic if it exists)
        // Separate classes and interfaces into a new source file and namespace
        // Slides and toggle needs an updating function that is called every time the menu is opened. this update needs to come from a delegate 

        private Matrix4x4 currentMatrix;
        private bool m_isActive = false;

        private MenuLayout menuLayout;

        private int propertySelectionIndex = 0;

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
            List<MenuProperty> _areaProperties = new List<MenuProperty>();
            _areaProperties.Add(new MenuProperty_Space(30));
            _areaProperties.Add(new MenuProperty_Box("Settings")); // Categories
            
            _areaProperties.Add(new MenuProperty_Label("Option Number one!"));

            _areaProperties.Add(new MenuProperty_Space(10));
            _areaProperties.Add(new MenuProperty_Box("Sound"));  // Categories

            _areaProperties.Add(new MenuProperty_Label("Option Number two!"));

            _areaProperties.Add(new MenuProperty_Space(50));
            _areaProperties.Add(new MenuProperty_Label("______________________________________________", Color.red));
            _areaProperties.Add(new MenuProperty_Space(5));
            _areaProperties.Add(new MenuProperty_Label("BUILD ID: XXXXXXX", Color.red));
            _areaProperties.Add(new MenuProperty_Label("BUILD DATE: 01-01-0101", Color.red));

            var newArea = new MenuArea
            {
                areaRect = new Rect(25, 25, 400, 600),
                areaName = "- DEBUG MENU -",
                areaProperties = _areaProperties
            };

            menuLayout.menuAreas.Add(newArea);
        }

        private void DrawDebugMenu()
        {
            if(!isActive)
                return;

            foreach(MenuArea _area in menuLayout.menuAreas)
            {
                // Draw menu
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

                GUILayout.EndArea();
            }
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