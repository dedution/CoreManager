using System;
using System.Collections;
using System.Collections.Generic;
using core.modules;
using Unity.Mathematics;
using UnityEngine;
using static core.GameManager;

namespace core.utils
{
    public class GameUtils
    {
        //Include static functions not only for external use but easy callbacks with game systems 
        public static void TestFunction()
        {
            
        }

        public static float CalculateFOV(int _horVer, float _camAspect)
        {
            float hFOVrad = _horVer * Mathf.Deg2Rad;
            float camH = Mathf.Tan(hFOVrad * 0.5f) / _camAspect;
            float vFOVrad = Mathf.Tan(camH) * 2f;
            return vFOVrad * Mathf.Rad2Deg;
        }

        // Handle logic of checking if points are inside camera UI space
        public static void CheckPointInUISpace()
        {
            
        }

        public static void SetTimeSlowMotion()
        {
            Game_SetGameSpeed(0.25f);
        }

        public static void SetTimeNormal()
        {
            Game_SetGameSpeed(1f);
        }

        public static void SetTimeFastMotion()
        {
            Game_SetGameSpeed(2f);
        }

        public static void StartTimer(string timerID, float duration, bool useUnscaledTime, Action onTimerEnd)
        {
            ActOnModule((TimeManager _ref) => {
                _ref.StartTimer(timerID, duration, useUnscaledTime, onTimerEnd);
            });
        }

        public static void StopTimer(string timerID)
        {
            ActOnModule((TimeManager _ref) => {
                _ref.StopTimer(timerID);
            });
        }

        public static float GetTimeRemaining(string timerID)
        {
            float time = 0f;

            ActOnModule((TimeManager _ref) => {
                time = _ref.GetTimeRemaining(timerID);
            });

            return time;
        }
    }
}