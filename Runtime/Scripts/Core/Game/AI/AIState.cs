using System.Collections;
using System.Collections.Generic;
using core.gameplay;
using UnityEngine;

namespace core.AI
{
    // Reference state
    public class AIState_Default : State<baseCharacterActor>
    {
        public AIState_Default()
        {
        }

        public override void EnterState(baseCharacterActor _npc)
        {
            Debug.Log(_npc.gameObject.name + " entered Default State.");
        }

        public override void ExitState(baseCharacterActor _npc)
        {
            Debug.Log(_npc.gameObject.name + " exited Default State.");
        }

        public override void UpdateState(baseCharacterActor _npc)
        {
            //Debug.Log(_npc.gameObject.name + " updating default State.");
        }
    }
}