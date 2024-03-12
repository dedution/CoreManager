using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
            
        }

        public static void SetTimeNormal()
        {
            
        }

        public static void SetTimeFastMotion()
        {
            
        }
    }
}