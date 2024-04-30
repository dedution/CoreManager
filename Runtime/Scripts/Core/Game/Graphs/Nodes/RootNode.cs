using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public class RootNode : Node
    {
        public Node child;

        protected override void onFinish()
        {
        }

        protected override void onStart()
        {
        }

        protected override State onUpdate()
        {
            return child.Update();
        }
    }
}