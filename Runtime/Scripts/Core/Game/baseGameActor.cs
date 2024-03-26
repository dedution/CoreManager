using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;
using static core.modules.SaveSystemManager;
using System;

namespace core.gameplay
{
    [System.Serializable]
    public struct SaveData
    {
        public bool Enabled;
        public string GUID;
        public Dictionary<string, object> Data;
        public void GenerateGUID()
        {
            if (string.IsNullOrWhiteSpace(GUID))
            {
                GUID = System.Guid.NewGuid().ToString();
            }
        }
    }

    public abstract class baseGameActor : MonoBehaviour
    {
        // Variables
        [Header("Actor Params")]
        public ActorTypes actorType = ActorTypes.System;
        public bool actorUpdatesViaManager = false;

        [Header("Save System")]
        public SaveData saveData = new SaveData();

        void Start()
        {
            // Try to init modules just in case
            GameManager.Instance.Init();

            // Register Actor
            GameManager.GetLoadedModule<ActorManager>().RegisterActor(this);

            // Load saved data (maybe add a way for a possible reload of actors data)
            SaveSystem_Load();

            // Initialize
            onStart();
        }

        protected virtual void onStart()
        {
        }

        public virtual void onUpdate()
        {
            // Called via Actor Manager Module
            // Use with caution
        }

        // Keep these two functions untouched for now
        protected void SaveSystem_Load()
        {
            GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Get(saveData);
        }

        protected void SaveSystem_Save()
        {
            GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Set(saveData);
        }

        protected T SaveSystem_GetData<T>(string _dataKey, T _defaultData)
        {
            // Object - Dictionary<string, object>
            // string is the key for the data that was saved
            // object is the value so we can have a dictionary of different value types
            var _data = saveData.Data != null && saveData.Data.ContainsKey(_dataKey) ? saveData.Data[_dataKey] : null;

            if (_data != null)
                return (T)_data;
            else
                return _defaultData;
        }

        protected void SaveSystem_SetData(string _dataKey, object _savedata)
        {
            if (saveData.Data == null)
                saveData.Data = new Dictionary<string, object>();

            if (saveData.Data.ContainsKey(_dataKey))
                saveData.Data[_dataKey] = _savedata;
            else
                saveData.Data.Add(_dataKey, _savedata);

            // Save this actor data into the save system
            SaveSystem_Save();
        }

        private void Destroy()
        {
            SaveSystem_Save();

            // Unregister Actor
            GameManager.GetLoadedModule<ActorManager>().UnregisterActor(this);

            onDestroy();
        }

        protected virtual void onDestroy()
        {

        }
    }
}