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

        public async override Task onExecute()
        {
            if (m_Chunk.ChunkState == WLoaderChunkStates.Unloaded)
            {
                m_Chunk.ChunkState = WLoaderChunkStates.Loading;

                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(m_Chunk.chunkID, LoadSceneMode.Additive, false);
                m_Chunk.sceneHandle = handle;

                await handle.Task;

                // One way to handle manual scene activation.
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    handle.Result.ActivateAsync();

                m_Chunk.ChunkState = WLoaderChunkStates.Loaded;
            }
        }
    }

    public class WLoaderTaskUnload : BaseTask
    {
        public WLoaderTaskUnload(LevelChunk _chunk)
        { 
            m_Chunk = _chunk;
        }

        public LevelChunk m_Chunk;

        public async override Task onExecute()
        {
            m_Chunk.ChunkState = WLoaderChunkStates.Unloading;
            
            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(m_Chunk.sceneHandle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            await handle.Task;

            // Maybe call an adressables free from memory handle?
            m_Chunk.ChunkState = WLoaderChunkStates.Unloaded;
        }
    }

    [System.Serializable]
    public struct LevelChunk
    {
        public string chunkID;
        public bool usesOptimization;
        public int activationsPerFrame;

        // Private and runtime only
        private WLoaderChunkStates currentChunkState;
        public WLoaderChunkStates ChunkState
        {
            get { return currentChunkState; }
            set { currentChunkState = value; }
        }

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

        public int GetLevelCount()
        {
            return Levels.Count;
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
        private bool LoaderInit = false;
        private bool LoaderIsBusy = false;
        private WorldConfig worldConfigData = new WorldConfig();

        public override void onInitialize()
        {
            // Load levels setup. This system won't work if config is missing
            LoadWorldConfig();
        }

        public void WriteTestConfig()
        {
            WorldConfig worldConfigTest = new WorldConfig();

            List<LevelChunk> chunks = new List<LevelChunk>();
            chunks.Add(new LevelChunk("Forest-Chunk-01", false));
            chunks.Add(new LevelChunk("Forest-Chunk-02", false));
            chunks.Add(new LevelChunk("Forest-Chunk-03", false));

            worldConfigTest.Levels.Add(new WorldLevel("Forest", chunks));
            
            // ASYNC WRITE IN NOT TAKING INTO ACCOUNT OVERRIDING
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

            if(worldConfigData.GetLevelCount() > 0)
                LoaderInit = true;
        }

        private void onTaskCompleted()
        {
            LoaderIsBusy = false;
            UpdateTasks();
        }

        public void LoadChunk(string levelID, string chunkID)
        {
            if(!worldConfigData.HasLevel(levelID) && chunkID != "")
                return;

            // Adds a load task for the target chunk
            LevelChunk _chunk = worldConfigData.GetChunk(levelID, chunkID);
            PushNewTask(new WLoaderTaskLoad(_chunk));
        }

        public void UnloadChunk(string levelID, string chunkID)
        {
            if(!worldConfigData.HasLevel(levelID) && chunkID != "")
                return;

            // Adds an unload task for the target chunk
            LevelChunk _chunk = worldConfigData.GetChunk(levelID, chunkID);
            PushNewTask(new WLoaderTaskUnload(_chunk));
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

        private void PushNewTask(BaseTask newTask)
        {
            if(newTask != null)
                loaderTasks.Enqueue(newTask);

            UpdateTasks();
        }

        private void UpdateTasks()
        {
            // Handling of task queue
            if (LoaderInit && loaderTasks.Count > 0 && !LoaderIsBusy)
            {
                LoaderIsBusy = true;
                BaseTask nextTask = loaderTasks.Dequeue();
                nextTask.Execute(onTaskCompleted).Start();
            }
        }

        public override void UpdateModule()
        {
            // UpdateTasks();
        }
    }
}