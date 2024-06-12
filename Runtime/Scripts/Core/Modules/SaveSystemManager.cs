using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using static core.GameManager;
using static core.IOController;

namespace core.modules
{
    [System.Serializable]
    public struct GameSaveData
    {
        // Pair of actor persistent id with pairs of data  
        public SaveDataPairs<string, SaveDataPairs<string, string>> GameData;
        public SaveDataPairs<string, string> ConfigData;

        public void InitData()
        {
            GameData = new SaveDataPairs<string, SaveDataPairs<string, string>>();
            ConfigData = new SaveDataPairs<string, string>();
        }
    }

    [System.Serializable]
    public class SaveDataPairs<Type, T>
    {
        public List<Type> keys;
        public List<T> values;

        public void AddData(Type _key, T _value)
        {
            if (keys == null || values == null)
            {
                keys = new List<Type>();
                values = new List<T>();

                keys.Add(_key);
                values.Add(_value);
            }
            else if (keys.Contains(_key))
                values[GetKeyID(_key)] = _value;
            else
            {
                keys.Add(_key);
                values.Add(_value);
            }
        }

        public bool ContainsKey(Type _key)
        {
            if (keys != null)
                return keys.Contains(_key);
            else
                return false;
        }

        private int GetKeyID(Type _key)
        {
            int id = keys.IndexOf(_key);
            return id;
        }

        public T GetData(Type _key, T _defaultData)
        {
            if (keys != null && keys.Contains(_key))
                return values[GetKeyID(_key)];
            else
                return _defaultData;
        }

        public T GetData(Type _key)
        {
            return values[GetKeyID(_key)];
        }
    }

    public class SaveSystemManager : BaseModule
    {
        public GameSaveData gameSaveData = new GameSaveData();

        public override void onInitialize()
        {
            gameSaveData.InitData();

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
            //EventManager.TriggerEvent("SubtitleManager", new Dictionary<string, object> { { "SETTINGS_ALLOWSUBTITLES", SaveSystem_Config_Get("SETTINGS_ALLOWSUBTITLES", false) } });
        }

        /// <summary>
        /// Saves into memory a pair of data. This pair is associated with a unique actor and consists of a key of type string and a value of type T. 
        /// </summary>
        public void SaveSystem_GameData_Set<T>(string actorGUID, string key, T data)
        {
            string _dataParse = DataToBinaryString(data);

            if (gameSaveData.GameData.ContainsKey(actorGUID))
                gameSaveData.GameData.GetData(actorGUID).AddData(key, _dataParse);
            else
            {
                gameSaveData.GameData.AddData(actorGUID, new SaveDataPairs<string, string>());
                gameSaveData.GameData.GetData(actorGUID).AddData(key, _dataParse);
            }
        }

        /// <summary>
        /// Returns a pair of data from memory with a specific key of type string 
        /// </summary>
        public T SaveSystem_GameData_Get<T>(string actorGUID, string key, T defaultData)
        {
            string _stringDefaultData = DataToBinaryString(defaultData);

            if (gameSaveData.GameData.ContainsKey(actorGUID))
            {
                string _rawdata = gameSaveData.GameData.GetData(actorGUID).GetData(key, _stringDefaultData);
                return BinaryStringToData<T>(_rawdata);
            }
            else
                return defaultData;
        }
        
        /// <summary>
        /// Tells Save System Module to save data to disk. This process is async and the SaveSystem event can be used to determine the current state 
        /// </summary>
        public void SaveSystem_Game_Save()
        {
            // Save data to file
            ActOnModule((EventManager _ref) => { _ref.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isSaving", true } }); });
            string _Path = GetSavePath();

            Debug.Log("Saving data to: " + _Path);

            IOController.WriteDataToFile<GameSaveData>(_Path, gameSaveData, true, true, onSaveDataSuccess);
        }

        /// <summary>
        /// Tells Save System Module to load data from disk into memory. This process is async and the SaveSystem event can be used to determine the current state 
        /// </summary>
        public void SaveSystem_Game_Load()
        {
            // Load and process data from file
            ActOnModule((EventManager _ref) => { _ref.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isLoading", true } }); });

            string _Path = GetSavePath();

            Debug.Log("Loading data from: " + _Path);

            IOController.ReadDataFromFile<GameSaveData>(_Path, true, onLoadDataSuccess);
        }

        private string GetSavePath()
        {
            string _Path = Path.Combine(Application.persistentDataPath, "savedata");

            if (!Directory.Exists(_Path))
                Directory.CreateDirectory(_Path);

            return Path.Combine(_Path, "data.dat");
        }

        private void onSaveDataSuccess()
        {
            Debug.Log("SAVE SUCESSFULL!");
            ActOnModule((EventManager _ref) => { _ref.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isSaving", false } }); });
        }

        private void onLoadDataSuccess(GameSaveData _data)
        {
            Debug.Log("LOAD SUCESSFULL!");
            gameSaveData = _data;
            ActOnModule((EventManager _ref) => { _ref.TriggerEvent("SaveSystem", new Dictionary<string, object> { { "isLoading", false } }); });
        }
    }
}