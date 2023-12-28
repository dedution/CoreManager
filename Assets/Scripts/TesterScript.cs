using System.Collections;
using System.Collections.Generic;
using core.modules;
using UnityEngine;

public class TesterScript : MonoBehaviour
{
    void Start()
    {
        GameManager.GetLoadedModule<DebugManager>().Hellow();

        //EventManager.StartListening("testevent", testEvent);

        Debug.Log("Test run!");

        //StartCoroutine(timertest());
    }

    IEnumerator timertest()
    {
        yield return new WaitForSeconds(1f);
        EventManager.TriggerEvent("testevent", null);
    }

    public void testEvent(Dictionary<string, object> param)
    {
        Debug.Log("Event listener reached!");
    }
}
