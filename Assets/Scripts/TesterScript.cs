using System.Collections;
using System.Collections.Generic;
using core.modules;
using UnityEngine;

public class TesterScript : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.Hellow();
        GameManager.Instance.GetLoadedModule<DebugManager>().Hellow();
        Debug.Log("Test run!");
    }
}
