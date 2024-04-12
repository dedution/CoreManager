using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        private float startTime;

        protected override void onFinish()
        {

        }

        protected override void onStart()
        {
            startTime = Time.time;
        }

        protected override State onUpdate()
        {
            if(Time.time - startTime < duration)
                return State.Running;
            else
                return State.Success;
        }
    }
}