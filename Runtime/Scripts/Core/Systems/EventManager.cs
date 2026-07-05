using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace core.modules
{
    /** How to use
    * EventManager.Trigger("gamePause", new Dictionary<string, object> { { "pause", true } });
    * 
    * EventManager.Connect("gamePause", OnGamePause);
    * 
    * EventManager.Disconnect("gamePause", OnGamePause);
    * 
    * void OnGamePause(Dictionary<string, object> param) 
    * {
    *   var isPaused = (bool) param["pause"];
    *   // Do logic with this flag
    * }
    */

    public class EventManager
    {
        private static Dictionary<string, Action<Dictionary<string, object>>> eventDictionary = new Dictionary<string, Action<Dictionary<string, object>>>();


        public static void Connect(string eventName, Action<Dictionary<string, object>> listener)
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

        public static void Disconnect(string eventName, Action<Dictionary<string, object>> listener)
        {
            Action<Dictionary<string, object>> thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent -= listener;
                eventDictionary[eventName] = thisEvent;
            }
        }

        public static void Trigger(string eventName, Dictionary<string, object> message)
        {
            Action<Dictionary<string, object>> thisEvent = null;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(message);
            }
        }
    }
}