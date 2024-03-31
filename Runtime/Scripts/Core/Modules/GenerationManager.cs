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

    public class GenerationManager : BaseModule
    {
        public GenerationManager()
        {
        }

        public override void onInitialize()
        {

        }
        
        public override void UpdateModule()
        {
        }

        public void GenerateDungeonLayout()
        {
            // Port world generation algorithm from Project Sphere
            // Optimize the dungeon generation algorithm for better performance and speed while 
            // baking probes, navmesh and such
        }
    }
}