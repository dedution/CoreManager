using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.graphs
{
    public abstract class CompositeNode : Node
    {
        public List<Node> children = new List<Node>();
    }
}