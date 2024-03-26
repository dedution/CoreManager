using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;

namespace core.modules
{
    public class SaveSystemManager : BaseModule
    {
        private Dictionary<string, object> m_LocalSaveData = new Dictionary<string, object>();
        private Dictionary<string, string> m_LocalConfigData = new Dictionary<string, string>();

        public override void onInitialize()
        {
            // Load config data immediately
            SaveSystem_LoadConfigData();
        }

        private void SaveSystem_LoadConfigData()
        {
            // Has to be async and have a saving state lockup
        }

        private void SaveSystem_SaveConfigData()
        {
            // Has to be async and have a saving state lockup

            // Use the EventManager to trigger this information to the necessary places
            EventManager.TriggerEvent("SubtitleManager", new Dictionary<string, object> { { "SETTINGS_ALLOWSUBTITLES", SaveSystem_Config_Get("SETTINGS_ALLOWSUBTITLES", false) } });
        }

        public string SaveSystem_Config_Get(string ID, string _default)
        {
            return m_LocalConfigData.ContainsKey(ID) ? m_LocalConfigData[ID] : _default;
        }

        public float SaveSystem_Config_Get(string ID, float _default)
        {
            return m_LocalConfigData.ContainsKey(ID) ? float.Parse(m_LocalConfigData[ID]) : _default;
        }

        public int SaveSystem_Config_Get(string ID, int _default)
        {
            return m_LocalConfigData.ContainsKey(ID) ? int.Parse(m_LocalConfigData[ID]) : _default;
        }

        public bool SaveSystem_Config_Get(string ID, bool _default)
        {
            return m_LocalConfigData.ContainsKey(ID) ? bool.Parse(m_LocalConfigData[ID]) : _default;
        }

        public void SaveSystem_Config_Set(string ID, string value)
        {
            // Save config immediately data
            if (m_LocalConfigData.ContainsKey(ID)) m_LocalConfigData[ID] = value;
            else
                m_LocalConfigData.Add(ID, value);

            SaveSystem_SaveConfigData();
        }

        public void SaveSystem_Config_Set(string ID, float value)
        {
            // Save config immediately data
            if (m_LocalConfigData.ContainsKey(ID)) m_LocalConfigData[ID] = value.ToString();
            else
                m_LocalConfigData.Add(ID, value.ToString());

            SaveSystem_SaveConfigData();
        }

        public void SaveSystem_Config_Set(string ID, int value)
        {
            // Save config immediately data
            if (m_LocalConfigData.ContainsKey(ID)) m_LocalConfigData[ID] = value.ToString();
            else
                m_LocalConfigData.Add(ID, value.ToString());

            SaveSystem_SaveConfigData();
        }

        public void SaveSystem_Config_Set(string ID, bool value)
        {
            // Save config immediately data
            if (m_LocalConfigData.ContainsKey(ID)) m_LocalConfigData[ID] = value.ToString();
            else
                m_LocalConfigData.Add(ID, value.ToString());

            SaveSystem_SaveConfigData();
        }

        public void SaveSystem_Game_Set(SaveData _actor)
        {
            if(!_actor.Enabled)
                return;
            
            if(m_LocalSaveData.ContainsKey(_actor.GUID))
                m_LocalSaveData[_actor.GUID] = _actor.Data;
            else
                m_LocalSaveData.Add(_actor.GUID, _actor.Data);
        }

        public void SaveSystem_Game_Get(SaveData _actor)
        {
            if(!_actor.Enabled)
                return;

            object _data = null; 

            if(m_LocalSaveData.ContainsKey(_actor.GUID))
                m_LocalSaveData.TryGetValue(_actor.GUID, out _data);

            _actor.Data = _data as Dictionary<string, object>;
        }

        public void SaveSystem_Game_Save()
        {
            // Save data to file
            EventManager.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isSaving", true } });
        }

        public void SaveSystem_Game_Load()
        {
            // Load and process data from file
            EventManager.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isLoading", true } });
        }
    }
}