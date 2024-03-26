using System.Collections;
using System.Collections.Generic;
using Generation.DynamicGrid;
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

        public GenerationManager()
        {
            if (_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {

        }

        public static void InitializeGridWorld(int _pieceSize, Camera _mainCamera)
        {
            _instance.m_GridWorldSystem = new GridWorldManager(_pieceSize, _mainCamera);
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

        public void GenerateInfiniteRunner()
        {
            // Port logic from Magia do Saber for the infinite runner
            // Consider moving that logic into a separate module when port is done
        }
    }
}