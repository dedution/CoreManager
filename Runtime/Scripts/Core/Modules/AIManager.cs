using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// TODO
// Handles logic for enemy and friendly NPCs such as enabling and resetting
// No pooling required. Character need to be loaded and unloaded with the scenes to prevent memory overloading
// Handle pathfinding optimizations (quality drop by distance)
// Handle animation controller optimizations (timeslicing frames by distance)

namespace core.modules
{
    public class AIManager : BaseModule
    {
        public override void onInitialize()
        {
            
        }
    }
}