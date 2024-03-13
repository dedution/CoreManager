using System.Collections;
using System.Collections.Generic;
using core.AI;
using UnityEngine;

namespace core.gameplay
{
    // Common logic for enemy and friendly NPCs
    // State machine brain should be defined here
    // Implement active ragdoll support with a separate controller or behavior
    // Path finding should be over time and async

    public abstract class baseCharacterActor : baseGameActor
    {
        protected AIStateMachine<baseCharacterActor> AIStateManager { get; set; }
        
        protected override void onStart()
        {
            base.onStart();

            AIStateManager = new AIStateMachine<baseCharacterActor>(this);

            // Get from AIManager the state objects for this character
            // AIStateManager.ChangeState(new AIState_Default());
        }

        public override void onUpdate()
        {
            // Update through actor manager
            base.onUpdate();
            AIStateManager.Update();
        }
    }
}