using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;


namespace core.debug.diagnostics
{
    public class DebugDiagnostics
    {
        // Framerate calculations
        private static float timer = 0f, refresh = 0f, avgFramerate = 0f;
        private static bool wasInitialized = false;

        private static ProfilerRecorder textureMemoryRecorder;
        private static ProfilerRecorder meshMemoryRecorder;
        private static ProfilerRecorder drawCallsRecorder;
        private static ProfilerRecorder shadowCastersRecorder;
        private static ProfilerRecorder trianglesRecorder;

        public static void InitDiagnostics()
        {
            wasInitialized = true;
        }

        public static void UpdateDiagnostics()
        {
            // Updates framerate
            UpdateFramerate();
        }

        public static void StartTrackingAssetMemory()
        {
            // Start recording memory used by textures
            textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");

            // Start recording memory used by meshes
            meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");

            // Start recording draw calls
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

            // Start recording shadow casters
            shadowCastersRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");

            trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        }

        public static void StopTrackingAssetMemory()
        {
            // Stop the recorders when the object is disabled
            textureMemoryRecorder.Dispose();
            meshMemoryRecorder.Dispose();
            drawCallsRecorder.Dispose();
            shadowCastersRecorder.Dispose();
            trianglesRecorder.Dispose();
        }

        public static long GetCurrentTriangles()
        {
            return trianglesRecorder.LastValue;
        }

        public static long GetCurrentDrawCalls()
        {
            return drawCallsRecorder.LastValue;
        }

        public static long GetCurrentShadowCasters()
        {
            return shadowCastersRecorder.LastValue;
        }

        public static float GetUsedMeshMemory()
        {
            return meshMemoryRecorder.LastValue / (1024f * 1024f);
        }

        public static float GetUsedTextureMemory()
        {
            return textureMemoryRecorder.LastValue / (1024f * 1024f);
        }

        public static long GetTotalReservedMemory()
        {
#if ENABLE_PROFILER
            return (long)(Profiler.GetTotalReservedMemoryLong() * 0.000001f);
#else
            return 0;
#endif
        }

        public static long GetAllocatedMemory()
        {
#if ENABLE_PROFILER
            return (long)(Profiler.GetTotalAllocatedMemoryLong() * 0.000001f);
#else
            return 0;
#endif
        }

        public static long GetReservedMemory()
        {
#if ENABLE_PROFILER
            return (long)(Profiler.GetTotalUnusedReservedMemoryLong() * 0.000001f);
#else
            return 0;
#endif
        }

        #region Framerate Calculations
        private static void UpdateFramerate()
        {
            float timelapse = Time.smoothDeltaTime;
            timer = timer <= 0 ? refresh : timer -= timelapse;

            if (timer <= 0) avgFramerate = (int)(1f / timelapse);
        }

        public static float GetCurrentFramerate()
        {
            if (!wasInitialized)
                Debug.LogError("Debug Diagnostics depends on the Debug Manager Module, but it wasn't initialized!");

            return avgFramerate;
        }

        #endregion
    }
}