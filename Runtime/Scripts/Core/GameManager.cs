using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using core.modules;

namespace core
{
    public class GameManager
    {
        private static GameManager _instance = null;

        //Persistent behavior that handles unity calls 
        private CoreDummyObject coreDummyObject;
        private ModuleController moduleController = new ModuleController();

        private GameManager()
        {
            //Initialize mono dummy gameobject
            coreDummyObject = new GameObject("MonoObject").AddComponent<CoreDummyObject>();
            UnityEngine.Object.DontDestroyOnLoad(coreDummyObject.gameObject);
        }

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameManager();

                return _instance;
            }
        }

        public void Init()
        {
            //Initialize
            moduleController.Init(coreDummyObject);
        }

        // Works but needs handling of this better or reworked to not use a list
        public static T GetLoadedModule<T>()
        {
            var _obj = Instance.moduleController.FindModule<T>();
            return (T)_obj; //Returns the first module found -- needs better handling in case of lack of modules
        }
    }
}