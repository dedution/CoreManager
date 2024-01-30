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

        private static EventManager _instance = null;

        public EventManager() {
            if(_instance == null)
                _instance = this;
            
            if (eventDictionary == null)
                eventDictionary = new Dictionary<string, Action<Dictionary<string, object>>>();
        }

        public override void onInitialize()
        {
            
        }

        public static void StartListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            if(_instance == null)
                return;
                
            Action<Dictionary<string, object>> thisEvent;

            if (_instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent += listener;
                _instance.eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                _instance.eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<Dictionary<string, object>> listener)
        {
            if(_instance == null)
                return;
                
            Action<Dictionary<string, object>> thisEvent;
            if (_instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent -= listener;
                _instance.eventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, Dictionary<string, object> message)
        {
            if(_instance == null)
                return;
                
            Action<Dictionary<string, object>> thisEvent = null;
            if (_instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(message);
            }
        }
    }
}