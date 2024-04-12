using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common.FsNodeReaders;
using core.AI;
using core.graphs;
using UnityEngine;

public class GraphTreeRunner : MonoBehaviour
{
    public GraphTree _tree;

    void Start()
    {
        _tree = ScriptableObject.CreateInstance<GraphTree>();

        var _node = ScriptableObject.CreateInstance<DebugLogNode>();
        _node.message = "TEST LOG!!";

        _tree.rootNode = _node;
    }

    void Update()
    {
        _tree.Update();
    }
}
