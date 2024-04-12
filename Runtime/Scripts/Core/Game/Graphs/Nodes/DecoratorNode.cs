using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public abstract class DecoratorNode : Node
    {
        public Node child;
    }
}