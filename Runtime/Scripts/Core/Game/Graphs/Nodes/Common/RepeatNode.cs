using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public class RepeatNode : DecoratorNode
    {
        protected override void onFinish()
        {

        }

        protected override void onStart()
        {
        }

        protected override State onUpdate()
        {
            child.Update();
            return State.Running;
        }
    }
}