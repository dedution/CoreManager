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
        public WLoaderTaskLoad(LevelChunk _chunk)
        { 
            m_Chunk = _chunk;
        }

        public LevelChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            if (m_Chunk.currentChunkState == WLoaderChunkStates.Unloaded)
            {
                m_Chunk.currentChunkState = WLoaderChunkStates.Loading;

                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(m_Chunk.chunkID, LoadSceneMode.Additive, false);
                m_Chunk.sceneHandle = handle;

                await handle.Task;

                // One way to handle manual scene activation.
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    handle.Result.ActivateAsync();

                m_Chunk.currentChunkState = WLoaderChunkStates.Loaded;
            }

            // call base to complete task
            base.Execute(onComplete);
        }
    }

    public class WLoaderTaskUnload : BaseTask
    {
        public WLoaderTaskUnload(LevelChunk _chunk)
        { 
            m_Chunk = _chunk;
        }

        public LevelChunk m_Chunk;

        public async override void Execute(Action onComplete)
        {
            m_Chunk.currentChunkState = WLoaderChunkStates.Unloading;
            
            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(m_Chunk.sceneHandle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            await handle.Task;

            // Maybe call an adressables free from memory handle?
            m_Chunk.currentChunkState = WLoaderChunkStates.Unloaded;
            base.Execute(onComplete);
        }
    }

    [System.Serializable]
    public struct LevelChunk
    {
        public string chunkID;
        public bool usesOptimization;
        public int activationsPerFrame;

        // Some variables and objects are not needed for serialization
        // Property to get this?
        public WLoaderChunkStates currentChunkState;
        public AsyncOperationHandle<SceneInstance> sceneHandle;

        public LevelChunk(string chunkID, bool usesOptimization = false, int activationsPerFrame = 3)
        {
            this.chunkID = chunkID; 
            this.usesOptimization = usesOptimization;
            this.activationsPerFrame = activationsPerFrame;
            sceneHandle = new AsyncOperationHandle<SceneInstance>();
            currentChunkState = WLoaderChunkStates.Unloaded;
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

        // Probably unnecessary if checking for bad data on task creation
        public bool HasLevel(string levelID)
        {
            bool _hasLevel = Levels.Where(level => level.levelID == levelID).Count() > 0;
            return _hasLevel;
        }

        public LevelChunk GetChunk(string levelID, string chunkID)
        {
            WorldLevel _level = Levels.Where(level => level.levelID == levelID).ElementAt(0);
            return _level.levelChunks.Where(chunk => chunk.chunkID == chunkID).ElementAt(0);
        }

        public List<string> GetLevelChunkIDs(string levelID)
        {
            List<string> _chunks = new List<string>();

            WorldLevel _level = Levels.Where(level => level.levelID == levelID).ElementAt(0);
            _level.levelChunks.ForEach(chunk => _chunks.Add(chunk.chunkID));
            
            return _chunks;
        }
    }

    // Handling of world data (Data to stream and load, light configuration, optimizations and etc)
    public class WorldManager : BaseModule
    {
        private Queue<BaseTask> loaderTasks = new Queue<BaseTask>();
        private bool LoaderIsBusy = false;
        private WorldConfig worldConfigData = new WorldConfig();

        public override void onInitialize()
        {
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
            worldConfigData = _data;
        }

        public void LoadChunk(string levelID, string chunkID)
        {
            if(!worldConfigData.HasLevel(levelID) && chunkID != "")
                return;

            // Adds a load task for the target chunk
            LevelChunk _chunk = worldConfigData.GetChunk(levelID, chunkID);
            loaderTasks.Enqueue(new WLoaderTaskLoad(_chunk));
        }

        public void UnloadChunk(string levelID, string chunkID)
        {
            if(!worldConfigData.HasLevel(levelID) && chunkID != "")
                return;

            // Adds an unload task for the target chunk
            LevelChunk _chunk = worldConfigData.GetChunk(levelID, chunkID);
            loaderTasks.Enqueue(new WLoaderTaskUnload(_chunk));
        }

        public void LoadLevel(string levelID)
        {
            // Creates loading tasks for all chunks associated with a level
            worldConfigData.GetLevelChunkIDs(levelID).ForEach(chunkID => LoadChunk(levelID, chunkID));
        }

        public void UnloadLevel(string levelID)
        {
            // Creates unloading tasks for all loaded chunks associated with a level
            worldConfigData.GetLevelChunkIDs(levelID).ForEach(chunkID => UnloadChunk(levelID, chunkID));
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