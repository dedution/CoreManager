using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Linq;

namespace core.modules
{
    // Using Addressables cause it handles memory dynamicly. Theres no need for a forced resource cleanup that freezes the main thread
    public enum WLoaderTaskTypes
    {
        Load,
        Unload
    }

    public enum WLoaderChunkStates
    {
        Unloaded,
        Loaded,
        Loading,
        Unloading
    }

    public abstract class WLoaderTask
    {
        public WLoaderTaskTypes taskType;

        // Make this function async
        public async virtual void Execute(Action onComplete)
        {
            // Call task completion
            if (onComplete != null)
                onComplete();

            await Task.Yield();
        }
    }

    public class WLoaderTaskLoad : WLoaderTask
    {
        public WLoaderTaskLoad()
        { }

        // Chunk to load
        public WLoaderChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            if (m_Chunk.m_currentState == WLoaderChunkStates.Unloaded)
            {
                m_Chunk.m_currentState = WLoaderChunkStates.Loading;

                for (int i = 0; i < m_Chunk.m_pieces.Length; i++)
                {
                    var _piece = m_Chunk.m_pieces[i];

                    AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(_piece.pieceID, LoadSceneMode.Additive, false);
                    _piece.sceneHandle = handle;

                    await handle.Task;

                    // One way to handle manual scene activation.
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                        handle.Result.ActivateAsync();

                    // Start Optimization process
                    // Find the optimizer at the root and make him start the timed activations
                    // Is it more performant to use the EventManager to trigger the optimizer instead of fetching the scene objects and triggering from there
                    // GameObject [] rootObjects = handle.Result.Scene.GetRootGameObjects();
                    // Maybe fetch the optimizer and wait for it to be finished here
                }

                m_Chunk.m_currentState = WLoaderChunkStates.Loaded;
            }

            // call base to complete task
            base.Execute(onComplete);
        }
    }

    public class WLoaderTaskUnload : WLoaderTask
    {
        public WLoaderTaskUnload()
        { }

        // Chunk to unload
        public WLoaderChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            m_Chunk.m_currentState = WLoaderChunkStates.Unloading;

            for (int i = 0; i < m_Chunk.m_pieces.Length; i++)
            {
                var _piece = m_Chunk.m_pieces[i];
                AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(_piece.sceneHandle, UnloadSceneOptions.None);

                await handle.Task;

                // One way to handle manual scene activation.
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    handle.Result.ActivateAsync();
            }

            m_Chunk.m_currentState = WLoaderChunkStates.Unloaded;
            base.Execute(onComplete);
        }
    }

    public struct WLoaderPiece
    {
        // Scene name
        public string pieceID;
        public AsyncOperationHandle<SceneInstance> sceneHandle;
    }

    public class WLoaderChunk
    {
        public string chunkID;
        // A chunk may contain several pieces (scenes) to load
        public WLoaderPiece[] m_pieces;
        public WLoaderChunkStates m_currentState;
        // Preload collisions
        public bool preloadCollisions = false;
    }

    // TODO
    // Handling of world data (Data to stream and load, light configuration, optimizations and etc)
    public class WorldManager : BaseModule
    {
        private Queue<WLoaderTask> wLoaderTasks = new Queue<WLoaderTask>();
        private bool wLoaderIsBusy = false;

        public override void onInitialize()
        {
            // Prepare the chunks and pieces necessary for the system to handle
            // wLoaderTasks.Enqueue(new WLoaderTaskLoad());
        }

        private void onTaskComplete()
        {
            wLoaderIsBusy = false;
        }

        public override void UpdateModule()
        {
            //Handling of task queue
            if (wLoaderTasks.Count > 0 && !wLoaderIsBusy)
            {
                WLoaderTask nextTask = wLoaderTasks.Dequeue();
                nextTask.Execute(onTaskComplete);
                wLoaderIsBusy = true;
            }
        }
    }
}