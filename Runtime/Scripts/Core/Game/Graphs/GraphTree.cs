using System.Collections;
using System.Collections.Generic;
using core.graphs;
using UnityEngine;

namespace core.graphs
{
    [CreateAssetMenu()]
    public class GraphTree : ScriptableObject
    {
        public Node rootNode;
        public Node.State treeState = Node.State.Running;

        public Node.State Update()
        {
            if(rootNode.state == Node.State.Running)
                treeState = rootNode.Update();
            
            return treeState;
        }

    }
}