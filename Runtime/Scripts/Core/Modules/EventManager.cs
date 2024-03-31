using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace core.modules
{
    /** How to use
    * TriggerEvent("gamePause", new Dictionary<string, object> { { "pause", true } });
    * 
    * StartListening("gamePause", OnGamePause);
    * 
    * StopListening("gamePause", OnGamePause);
    * 
    * void OnGamePause(Dictionary<string, object> param) 
    * {
    *   var isPaused = (bool) param["pause"];
    *   // Do logic with this flag
    * }
    *
    * TODO:
    * Add the implementation for a timer in secs for triggering delay 
    *
    */

    public class EventManager : BaseModule
    {
        private Dictionary<string, Action<Dictionary<string, object>>> eventDictionary;

        public override void onInitialize()
        {
            eventDictionary = new Dictionary<string, Action<Dictionary<string, object>>>();
        }

        public void StartListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            Action<Dictionary<string, object>> thisEvent;

            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent += listener;
                eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        public void StopListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            Action<Dictionary<string, object>> thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent -= listener;
                eventDictionary[eventName] = thisEvent;
            }
        }

        public void TriggerEvent(string eventName, Dictionary<string, object> message)
        {
            Action<Dictionary<string, object>> thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(message);
            }
        }
    }
}