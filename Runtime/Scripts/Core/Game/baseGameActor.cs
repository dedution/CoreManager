using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;

namespace core.gameplay
{
    public abstract class baseGameActor : MonoBehaviour, ISaver
    {
        // Variables
        public object SaveData { get; set; }
        public bool SaveSystem_Enabled { get; set; }
        public string SaveSystem_GUID  { get; set; }

        void Start() 
        {
            // Register Actor
            GameManager.GetLoadedModule<ActorManager>().RegisterActor(this);

            // Load saved data
            SaveSystem_Load();
        }

        public virtual void SaveSystem_Load()
        {
            if(!SaveSystem_Enabled)
                return;

            SaveData = GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Get(this);
        }

        public virtual void SaveSystem_Save()
        {
            if(!SaveSystem_Enabled)
                return;

            GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Set(this);
        }

        void OnEnable()
        {
            GenerateGUID();
        }

        void OnValidate()
        {
            GenerateGUID();
        }

        private void GenerateGUID()
        {
            if (string.IsNullOrWhiteSpace(SaveSystem_GUID)) {
                SaveSystem_GUID = System.Guid.NewGuid().ToString();
            }
        }

        public string GetGUID()
        {
            return SaveSystem_GUID;
        }

        void Destroy()
        {
            SaveSystem_Save();

            // Unregister Actor
            GameManager.GetLoadedModule<ActorManager>().UnregisterActor(this);
        }
    }
}