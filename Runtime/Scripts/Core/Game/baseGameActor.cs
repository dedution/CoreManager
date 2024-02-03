using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using core.modules;
using static core.modules.SaveSystemManager;
using System;

namespace core.gameplay
{
    public class baseGameActor : MonoBehaviour
    {
        // Variables
        [Header("Actor Params")]
        public ActorTypes actorType = ActorTypes.System;

        [Header("Save System")]
        public SaveData saveData = new SaveData();

        void Start()
        {
            // Try to init modules just in case
            GameManager.Instance.Init();

            // Register Actor
            GameManager.GetLoadedModule<ActorManager>().RegisterActor(this);

            // Load saved data -- maybe call this through an event for save system updates?
            SaveSystem_Load();

            // Initialize
            onStart();
        }

        protected virtual void onStart()
        {
        }

        protected virtual void SaveSystem_Load()
        {
            GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Get(saveData);
        }

        protected virtual void SaveSystem_Save()
        {
            GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Game_Set(saveData);
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