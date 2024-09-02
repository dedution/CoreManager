using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using static core.GameManager;

// TODO
// INSTEAD OF CREATING AND DESTROYING TIMER OBJECTS AND RESULTING IN GARBAGE TO BE COLLECTED
// MAKE A POOL OF REUSABLE TIMERS INSTEAD
namespace core.modules
{
    class Timer
    {
        public string ID;
        public float Duration;
        public float TimeRemaining;
        public Action OnTimerEnd;
        public bool IsRunning;
        public bool UseUnscaledTime = false;

        public Timer(string id, float duration, Action onTimerEnd, bool useUnscaledTime = false)
        {
            ID = id;
            Duration = duration;
            TimeRemaining = duration;
            OnTimerEnd = onTimerEnd;
            IsRunning = true;
            UseUnscaledTime = useUnscaledTime;
        }

        public void Update(float deltaTime)
        {
            if (IsRunning)
            {
                TimeRemaining -= deltaTime;
                if (TimeRemaining <= 0f)
                {
                    TimeRemaining = 0f;
                    IsRunning = false;
                    OnTimerEnd?.Invoke();
                }
            }
        }
    }

    public class TimeManager : BaseModule
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

        public override void onInitialize()
        {

        }

        public override void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {
            // Update all active timers
            foreach (var timer in new List<Timer>(timers.Values))
            {
                timer.Update(timer.UseUnscaledTime ? unscaledDeltaTime : deltaTime);

                if (!timer.IsRunning)
                {
                    timers.Remove(timer.ID);
                }
            }
        }

        public void StartTimer(string timerID, float duration, bool useUnscaledTime, Action onTimerEnd)
        {
            if (timers.ContainsKey(timerID))
            {
                Debug.LogWarning($"Timer with ID {timerID} already exists. Timer not started.");
                return;
            }

            Timer newTimer = new Timer(timerID, duration, onTimerEnd, useUnscaledTime);
            timers.Add(timerID, newTimer);
        }

        public void StopTimer(string timerID)
        {
            if (timers.TryGetValue(timerID, out Timer timer))
            {
                timer.IsRunning = false;
                timers.Remove(timerID);
            }
        }

        public void StopAllTimers()
        {
            timers.Clear();
        }

        public float GetTimeRemaining(string id)
        {
            if (timers.TryGetValue(id, out Timer timer))
            {
                return timer.TimeRemaining;
            }

            return 0f;
        }
    }
}