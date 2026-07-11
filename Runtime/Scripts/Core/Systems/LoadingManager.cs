using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.modules
{
    public static class LoadingManager
    {
        private static readonly Dictionary<string, ILoadingOperation> loadingOperations = new();

        public static void Begin()
        {
            EventManager.Trigger("StartLoading");
            GameManager.RunCoroutine(ExecuteAll());
        }

        public static void AddCheckpoint(string checkpointName, ILoadingOperation operation)
        {
            if (loadingOperations.ContainsKey(checkpointName))
            {
                Debug.LogError($"Loading checkpoint '{checkpointName}' is already registered.");
                return;
            }

            loadingOperations.Add(checkpointName, operation);
        }

        private static IEnumerator ExecuteAll()
        {
            foreach (var operation in loadingOperations.Values)
            {
                IEnumerator routine = operation.Execute();

                while (routine.MoveNext())
                {
                    EmitProgress();
                    yield return routine.Current;
                }

                EmitProgress();
            }

            End();
        }

        private static void EmitProgress()
        {
            float totalWeight = 0f;
            float completedWeight = 0f;

            foreach (var operation in loadingOperations.Values)
            {
                totalWeight += Mathf.Max(0f, operation.Weight);
                completedWeight += Mathf.Clamp01(operation.Progress) * Mathf.Max(0f, operation.Weight);
            }

            float progress = totalWeight <= 0f
                ? 100f
                : (completedWeight / totalWeight) * 100f;

            EventManager.Trigger("LoadingProgressUpdate",
                new Dictionary<string, object>
                {
                    { "progress", progress }
                });
        }

        public static float Progress
        {
            get
            {
                float totalWeight = 0f;
                float completedWeight = 0f;

                foreach (var operation in loadingOperations.Values)
                {
                    totalWeight += Mathf.Max(0f, operation.Weight);
                    completedWeight += Mathf.Clamp01(operation.Progress) * Mathf.Max(0f, operation.Weight);
                }

                return totalWeight <= 0f
                    ? 1f
                    : completedWeight / totalWeight;
            }
        }

        public static bool IsLoading => loadingOperations.Count > 0;

        private static void End()
        {
            EmitProgress();

            EventManager.Trigger("EndLoading");

            loadingOperations.Clear();
        }
    }
}