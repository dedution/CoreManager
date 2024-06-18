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
using core.tasks;
using System.IO;

namespace core.modules
{
    public enum WLoaderChunkStates
    {
        Unloaded,
        Loaded,
        Loading,
        Unloading
    }

    public class WLoaderTaskLoad : BaseTask
    {
        public WLoaderTaskLoad()
        { }

        public LevelChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            // if (m_Chunk.m_currentState == WLoaderChunkStates.Unloaded)
            // {
            //     m_Chunk.m_currentState = WLoaderChunkStates.Loading;

            //     for (int i = 0; i < m_Chunk.m_pieces.Length; i++)
            //     {
            //         var _piece = m_Chunk.m_pieces[i];

            //         AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(_piece.pieceID, LoadSceneMode.Additive, false);
            //         _piece.sceneHandle = handle;

            //         await handle.Task;

            //         // One way to handle manual scene activation.
            //         if (handle.Status == AsyncOperationStatus.Succeeded)
            //             handle.Result.ActivateAsync();

            //         // Start Optimization process
            //         // Find the optimizer at the root and make him start the timed activations
            //         // Is it more performant to use the EventManager to trigger the optimizer instead of fetching the scene objects and triggering from there
            //         // GameObject [] rootObjects = handle.Result.Scene.GetRootGameObjects();
            //         // Maybe fetch the optimizer and wait for it to be finished here
            //     }

            //     m_Chunk.m_currentState = WLoaderChunkStates.Loaded;
            // }

            // call base to complete task
            base.Execute(onComplete);
        }
    }

    public class WLoaderTaskUnload : BaseTask
    {
        public WLoaderTaskUnload()
        { }

        // Chunk to unload
        public LevelChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            // m_Chunk.m_currentState = WLoaderChunkStates.Unloading;

            // for (int i = 0; i < m_Chunk.m_pieces.Length; i++)
            // {
            //     var _piece = m_Chunk.m_pieces[i];
            //     AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(_piece.sceneHandle, UnloadSceneOptions.None);

            //     await handle.Task;

            //     // One way to handle manual scene activation.
            //     if (handle.Status == AsyncOperationStatus.Succeeded)
            //         handle.Result.ActivateAsync();
            // }

            // m_Chunk.m_currentState = WLoaderChunkStates.Unloaded;
            base.Execute(onComplete);
        }
    }

    public struct WLoaderChunk
    {
        public string pieceID;
        public AsyncOperationHandle<SceneInstance> sceneHandle;

        public WLoaderChunk(string pieceID)
        {
            this.pieceID = pieceID;
            sceneHandle = new AsyncOperationHandle<SceneInstance>();
        }
    }
    
    [System.Serializable]
    public struct LevelChunk
    {
        public string chunkID;
        public bool isOptimization;

        public LevelChunk(string chunkID, bool isOptimization)
        {
            this.chunkID = chunkID; 
            this.isOptimization = isOptimization;
        }
    }

    [System.Serializable]
    public struct WorldLevel
    {
        public string levelID;
        public List<LevelChunk> levelChunks;

        public WorldLevel(string levelID, List<LevelChunk> levelChunks)
        {
            this.levelID = levelID;
            this.levelChunks = levelChunks;
        }
    }

    [System.Serializable]
    public class WorldConfig
    {
        public List<WorldLevel> Levels = new List<WorldLevel>();
    }

    // Handling of world data (Data to stream and load, light configuration, optimizations and etc)
    public class WorldManager : BaseModule
    {
        private Dictionary<string, WLoaderChunk> worldLoaderData = new Dictionary<string, WLoaderChunk>();
        private Queue<BaseTask> loaderTasks = new Queue<BaseTask>();
        private bool LoaderIsBusy = false;

        public override void onInitialize()
        {
            // Prepare the chunks and pieces necessary for the system to handle
            // wLoaderTasks.Enqueue(new WLoaderTaskLoad());
            // WriteTestConfig();
            LoadWorldConfig();
        }

        private void WriteTestConfig()
        {
            WorldConfig worldConfigTest = new WorldConfig();

            List<LevelChunk> chunks = new List<LevelChunk>();
            chunks.Add(new LevelChunk("Forest-Chunk-01", false));
            chunks.Add(new LevelChunk("Forest-Chunk-02", false));
            chunks.Add(new LevelChunk("Forest-Chunk-03", false));

            List<WorldLevel> levels = new List<WorldLevel>();
            levels.Add(new WorldLevel("Forest", chunks));
            
            worldConfigTest.Levels = levels;
            
            string _Path = Path.Combine(Application.streamingAssetsPath, "worldconfig.json");
            IOController.WriteJSONToFile(_Path, worldConfigTest, true, true);
        }

        private void LoadWorldConfig()
        {
            string _Path = Path.Combine(Application.streamingAssetsPath, "worldconfig.json");

            // Only load the config file if it exists
            if(File.Exists(_Path))
                IOController.ReadJSONFromFile<WorldConfig>(_Path, true, onWorldConfigLoaded);
        }

        private void onWorldConfigLoaded(WorldConfig _data)
        {
            ParseWorldDataToPieces(_data);
        }

        private void ParseWorldDataToPieces(WorldConfig _worldData)
        {
            // Careful with this, can contain asset handlers
            worldLoaderData.Clear();

            foreach(var level in _worldData.Levels) {
                foreach(var chunk in level.levelChunks) {
                    worldLoaderData.Add(level.levelID, new WLoaderChunk(chunk.chunkID));
                }
            }
        }

        public override void UpdateModule()
        {
            // Handling of task queue
            if (loaderTasks.Count > 0 && !LoaderIsBusy)
            {
                BaseTask nextTask = loaderTasks.Dequeue();
                nextTask.Execute(()=> { LoaderIsBusy = false; });
                LoaderIsBusy = true;
            }
        }
    }
}