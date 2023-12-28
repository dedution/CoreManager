using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using core.modules;

public class GameManager
{
    //Never use this variable directly
    private static GameManager _instance = null;
    
    //Persistent behavior that handles unity calls 
    private CoreDummyObject coreDummyObject;

    private List<BaseModule> activeModules = new List<BaseModule>();

    private GameManager()
    {
        Debug.Log("GameManager Initialized.");
        
        //Initialize mono dummy gameobject
        coreDummyObject = new GameObject("DummyObject").AddComponent<CoreDummyObject>();
        UnityEngine.Object.DontDestroyOnLoad(coreDummyObject.gameObject);

        //Initialize modules - uses dummy object namespace to find other classes in the same namepsace and inits them
        foreach(BaseModule _module in InstantiateTypesInSameNamespaceAs<BaseModule>(coreDummyObject))
        {
            activeModules.Add(_module);
            //Register unity calls to delegates?
        }
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

    public void Hellow()
    {
        //Testing call
    }

    // Works but needs handling of this better or reworked to not use a list
    public static T GetLoadedModule<T>()
    {
        var _obj = Instance.activeModules.OfType<T>().ToList();
        return (T)_obj[0]; //Returns the first module found -- needs better handling in case of lack of modules
    }

    private List<Type> FindTypesInSameNamespaceAs(object instance)
    {
        string ns = instance.GetType().Namespace;
        Type instanceType = instance.GetType();
        List<Type> results = instance.GetType().Assembly.GetTypes().Where(tt => tt.Namespace == ns &&
                                                                          tt != instanceType).ToList();
        return results;
    }

    private List<T> InstantiateTypesInSameNamespaceAs<T>(object instance)
    {
        List<T> instances = new List<T>();

        foreach (Type t in FindTypesInSameNamespaceAs(instance))
        {
            if (t.IsSubclassOf(typeof(T)))
            {
                T i =(T) Activator.CreateInstance(t);
                instances.Add(i);
            }
        }

        return instances;
    }
}