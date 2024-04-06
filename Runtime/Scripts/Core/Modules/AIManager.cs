using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.AI;
using System.Threading.Tasks;

// TODO
// Handles logic for enemy and friendly NPCs such as enabling and resetting
// No pooling required. Character need to be loaded and unloaded with the scenes to prevent memory overloading
// Handle pathfinding optimizations (quality drop by distance)
// Handle animation controller optimizations (timeslicing frames by distance)
// Handles AI patrol paths (TBI)

namespace core.modules
{
    public class AIManager : BaseModule
    {
        //private Queue<object> AICalculationQueue = new Queue<object>();
        
        // Timeout when task is waiting some process to finish (in milliseconds)
        private const int TASKDELAYTIMEOUT = 10;

        // Number of tasks running simultaneously
        private const int SIMULTANEOUSTASKS = 1;

        public override void onInitialize()
        {
            
        }

        private async void UpdateNavMeshAgent(NavMeshAgent navMeshAgent, Vector3 _target)
        {
            navMeshAgent.SetDestination(_target);

            while(navMeshAgent.pathPending)
                await Task.Delay(TASKDELAYTIMEOUT); // wait 10 mil
        }
    }
}