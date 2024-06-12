using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.debug.layouts
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

        bool MenuProperty.isSelectable { get { return false; } }

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
            if (isStart)
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, gUILayoutOption1, gUILayoutOption2);
            else
                GUILayout.EndScrollView();
        }
    }

    public class MenuProperty_Box : MenuProperty
    {
        string _data = "";
        bool MenuProperty.isSelectable { get { return false; } }

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
        bool MenuProperty.isSelectable { get { return false; } }

        public MenuProperty_Horizontal(bool isStart)
        {
            this.isStart = isStart;
        }

        public void RunProperty()
        {
            if (isStart)
                GUILayout.BeginHorizontal();
            else
                GUILayout.EndHorizontal();
        }
    }

    public class MenuProperty_Toggle : MenuProperty
    {
        bool val = false;
        string label = "";

        public Action<bool> OnUpdate;
        bool MenuProperty.isSelectable { get { return false; } }

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
        bool MenuProperty.isSelectable { get { return false; } }

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
        bool MenuProperty.isSelectable { get { return _isSelectable; } }

        bool _isSelectable = false;

        public GUILayoutOption GUILayoutOption { get; }

        public MenuProperty_Label(string _customData, bool isSelectable = false)
        {
            _data = _customData;

            _isSelectable = isSelectable;
        }

        public MenuProperty_Label(string _customData, Color color, bool isSelectable = false)
        {
            _data = _customData;

            if (color != null)
                this.color = color;

            _isSelectable = isSelectable;
        }

        public MenuProperty_Label(string _customData, GUILayoutOption gUILayoutOption, Color color, string style = "", bool isSelectable = false) : this(_customData)
        {
            _data = _customData;
            GUILayoutOption = gUILayoutOption;
            _style = style;

            if (color != null)
                this.color = color;

            _isSelectable = isSelectable;
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

            if (timerDir)
                timer -= Time.unscaledDeltaTime * 2f;
            else
                timer += Time.unscaledDeltaTime * 2f;

            if (timer > 1f)
            {
                timerDir = true;
            }
            else if (timer < 0f)
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
        public float currentValue { get; set; }
        public float minValue { get; }
        public float maxValue { get; }
        bool MenuProperty.isSelectable { get { return false; } }

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
        bool isSelectable { get; }

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
}

