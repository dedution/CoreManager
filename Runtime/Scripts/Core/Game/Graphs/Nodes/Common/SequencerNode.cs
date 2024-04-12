using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public class SequencerNode : CompositeNode
    {
        int current;

        protected override void onFinish()
        {

        }

        protected override void onStart()
        {
            current = 0;
        }

        protected override State onUpdate()
        {
            var currentChild = children[current];

            switch(currentChild.Update())
            {
                case State.Fail:
                    return State.Fail;
                case State.Running:
                    return State.Running;
                case State.Success:
                    {
                        current++;
                        break;
                    }
            }

            return current == children.Count ? State.Success : State.Running;
        }
    }
}