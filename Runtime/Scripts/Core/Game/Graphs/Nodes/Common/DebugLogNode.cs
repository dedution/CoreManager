using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public class DebugLogNode : ActionNode
    {
        public string message = "";
        
        protected override void onFinish()
        {
            // No use here
        }

        protected override void onStart()
        {
            Debug.Log(message);
        }

        protected override State onUpdate()
        {
            return State.Running;
        }
    }
}