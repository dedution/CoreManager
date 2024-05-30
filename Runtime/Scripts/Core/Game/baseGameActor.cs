using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;
using System;
using static core.GameManager;

namespace core.gameplay
{
    [System.Serializable]
    public struct ActorsaveData
    {
        public bool Enabled;
        public string GUID;
        public void GenerateGUID()
        {
            if (string.IsNullOrWhiteSpace(GUID))
            {
                GUID = System.Guid.NewGuid().ToString();
            }
        }
    }

    public class baseGameActor : MonoBehaviour
    {
        // Variables
        [Header("Actor Params")]
        //public ActorTypes actorType = ActorTypes.System;
        public bool actorUpdatesViaManager = false;

        [Header("Save System")]
        public ActorsaveData saveData = new ActorsaveData();

        void Start()
        {
            // Try to init game manager
            Init();

            // Register Actor
            ActOnModule((ActorManager _ref) => {_ref.RegisterActor(this);});

            // Initialize actor
            onStart();
        }

        protected virtual void onStart()
        {}

        // Called via Actor Manager Module
        public virtual void onUpdate()
        {}

        protected T SaveSystem_GetData<T>(string _dataKey, T _defaultData)
        {
            T _data = _defaultData;

            if (saveData.Enabled)
                ActOnModule((SaveSystemManager _ref) => { _data = _ref.SaveSystem_GameData_Get(saveData.GUID, _dataKey, _defaultData); });

            return _data;
        }

        protected void SaveSystem_SetData<T>(string _dataKey, T _savedata)
        {
            if (saveData.Enabled)
                ActOnModule((SaveSystemManager _ref) => { _ref.SaveSystem_GameData_Set(saveData.GUID, _dataKey, _savedata); });
        }

        private void Destroy()
        {
            // Unregister Actor
            ActOnModule((ActorManager _ref) => {_ref.UnregisterActor(this);});

            onDestroy();
        }

        protected virtual void onDestroy()
        {

        }
    }
}