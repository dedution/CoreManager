using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using core.gameplay;
using System.Threading.Tasks;

namespace core.modules
{
    public enum WLoaderTaskTypes
    {
        Load,
        Unload,
        // Optimization Pass relates to the goal of slicing the activation of gameobjects and their own initialization (these factors cause stuterring on the engine side)
        OptimizationPass
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
        // Ideal for imposters. Trying to keep the world as seamless as possible.
        public bool loadLowVersion = false;

        public async override void Execute(Action onComplete)
        {
            await Task.Yield();
            base.Execute(onComplete);
        }
    }

    public class WLoaderTaskUnload : WLoaderTask
    {
        public WLoaderTaskUnload()
        { }

        // Chunk to unload
        public string chunkID;

        public async override void Execute(Action onComplete)
        {
            await Task.Yield();
            base.Execute(onComplete);
        }
    }

    public struct WLoaderPiece
    {
        // Scene name
        string pieceID;
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
            wLoaderTasks.Enqueue(new WLoaderTaskLoad());
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