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

        public static Coroutine Lerp(Action<float> target, float initial_value, float target_value, float duration = 1.0f, Coroutine previous = null, Action finish_callback = null)
        {
            if(previous != null)
                StopCoroutine(previous);

            return RunCoroutine(LerpCoroutine(target, initial_value, target_value, duration, finish_callback));
        }

        private static IEnumerator LerpCoroutine(Action<float> target, float initial_value, float target_value, float duration = 1.0f, Action finish_callback = null)
        {
            float origin = initial_value;
            float elapsedTime = 0;

            // Wait until the time has reached the target duration.
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                target(Mathf.Lerp(origin, target_value, elapsedTime / duration));
                yield return null;
            }
            
            target(target_value);
            finish_callback?.Invoke();
        }

        public static float FOVConverter(float horizontalFOV, float aspectRatio = 16f / 9f)
        {
            // Convert horizontal FOV to radians
            float horizontalFOVRadians = horizontalFOV * Mathf.Deg2Rad / 2f;

            // Calculate vertical FOV using the formula
            float verticalFOVRadians = 2f * Mathf.Atan(Mathf.Tan(horizontalFOVRadians) / aspectRatio);

            // Convert back to degrees
            float verticalFOV = verticalFOVRadians * Mathf.Rad2Deg;

            return verticalFOV;
        }

        public static void SetGameSpeed(float _targetSpeed)
        {
            Time.timeScale = _targetSpeed;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }

        public static float GetGameSpeed()
        {
            return Time.timeScale;
        }
    }
}