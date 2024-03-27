using System.Collections;
using System.Collections.Generic;
using Generation.DynamicGrid;
using Generation.RunnerWorld;
using UnityEngine;

namespace core.modules
{
    // TODO
    // Responsable of having logic for easy world generation
    // Dungeon generation algorithms (easier interior world building) - refer 
    // Terrain generation algorithms
    // Perhaps separate the Dynamic Grid World logic into a single module 

    public class GenerationManager : BaseModule
    {
        private static GenerationManager _instance = null;
        private GridWorldManager m_GridWorldSystem;
        private RunnerWorldController m_RunnerWorldSystem;

        public GenerationManager()
        {
            if (_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {

        }
        
        public override void UpdateModule()
        {
            if(!ReferenceEquals(_instance.m_GridWorldSystem, null))
                _instance.m_GridWorldSystem.onUpdate();

            if(!ReferenceEquals(_instance.m_RunnerWorldSystem, null))
                _instance.m_RunnerWorldSystem.onUpdate();
        }

        public static void InitializeRunnerWorld(GameObject _prefab, int _pieceSize, int _pieceNumber)
        {
            _instance.m_RunnerWorldSystem = new RunnerWorldController(_prefab, _pieceSize, _pieceNumber);
        }

        public static void UpdateRunnerWorldSpeed(float speed)
        {
            if(!ReferenceEquals(_instance.m_RunnerWorldSystem, null))
                _instance.m_RunnerWorldSystem.m_MoveSpeed = speed;
        }

        public static void InitializeGridWorld(int _pieceSize, Camera _mainCamera)
        {
            _instance.m_GridWorldSystem = new GridWorldManager(_pieceSize, _mainCamera);
        }

        public static RunnerWorldController GetRunnerWorldManager()
        {
            if(_instance.m_RunnerWorldSystem == null)
                Debug.LogError("Runner World Manager wasn't initialized!");

            return _instance.m_RunnerWorldSystem;
        }

        public static GridWorldManager GetGridWorldManager()
        {
            if(_instance.m_GridWorldSystem == null)
                Debug.LogError("Grid World Manager wasn't initialized!");

            return _instance.m_GridWorldSystem;
        }

        public void GenerateDungeonLayout()
        {
            // Port world generation algorithm from Project Sphere
        }
    }
}