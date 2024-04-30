using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using core.graphs;
using UnityEngine;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Node node;
    public UnityEditor.Experimental.GraphView.Port input;
    public UnityEditor.Experimental.GraphView.Port output;

    public NodeView(Node node)
    {
        this.node = node;
        this.title = node.name;
        style.left = node.position.x;
        style.top = node.position.y;
        this.viewDataKey = node.guid;

        CreateInputPorts();
        CreateOutputPorts();
    }

    private void CreateInputPorts()
    {
        if(node is ActionNode)
        {
            input = InstantiatePort(UnityEditor.Experimental.GraphView.Orientation.Horizontal, 
            UnityEditor.Experimental.GraphView.Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(bool));
        }

        if(input != null)
        {
            // Naming ports?
            input.portName = "";
            inputContainer.Add(input);
        }
    }

    private void CreateOutputPorts()
    {
        if(output != null)
        {
            // Naming ports?
            output.portName = "";
            outputContainer.Add(output);
        }   
    }
    public override void SetPosition(Rect rect)
    {
        base.SetPosition(rect);
        node.position.x = rect.xMin;
        node.position.y = rect.yMin;

    }
}
