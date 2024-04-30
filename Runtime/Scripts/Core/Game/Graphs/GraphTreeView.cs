using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using core.graphs;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<GraphTreeView, GraphView.UxmlTraits> { }
    GraphTree tree;

    public GraphTreeView()
    {
        Insert(0, new GridBackground());

        // View Manipulation
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        // Packages/com.dedution.coremanager/

        bool isPackage = false;
        var pathPrefix = isPackage ? "Packages/com.dedution.coremanager/" : "Assets/CoreManager/";

        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(pathPrefix + "Runtime/Scripts/Core/Game/Graphs/GraphTreeEditor.uss");
        styleSheets.Add(stylesheet);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // Maybe append an action for node deletion
        // and other node contextual
        {
            var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }

        {
            var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }

        {
            var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChanged)
    {
        if(graphViewChanged.elementsToRemove != null)
        {
            graphViewChanged.elementsToRemove.ForEach(elem => {
                NodeView node = elem as NodeView;

                if(node != null)
                    tree.DeleteNode(node.node);


                Edge edge = elem as Edge;

                if(edge != null)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.RemoveChild(parentView.node, childView.node);
                }
            });
        }

        if(graphViewChanged.edgesToCreate != null)
        {
            graphViewChanged.edgesToCreate.ForEach(elem => {
                NodeView parentView = elem.output.node as NodeView;
                NodeView childView = elem.input.node as NodeView;

                tree.AddChild(parentView.node, childView.node);
            });
        }

        return graphViewChanged;
    }
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endport => endport.direction != startPort.direction && endport.node != startPort.node).ToList();
    }

    private void CreateNode(Type type)
    {
        core.graphs.Node node = tree.CreateNode(type);
        CreateNodeView(node);
    }
    
    public void PopulateView(GraphTree tree)
    {
        this.tree = tree;
        
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements); 
        graphViewChanged += OnGraphViewChanged;

        if(tree.rootNode == null)
        {
            tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        // Populate nodes
        tree.nodes.ForEach(n => CreateNodeView(n));

        // Populate edges
        tree.nodes.ForEach(n => {
            var children = tree.GetChildren(n);
            children.ForEach(c => {
                NodeView parent = FindNodeview(n);
                NodeView child = FindNodeview(c);

                Edge edge = parent.output.ConnectTo(child.input);
                AddElement(edge);

            });
        });

    }

    private void CreateNodeView(core.graphs.Node node)
    {
        NodeView nodeView = new NodeView(node);
        AddElement(nodeView);
    }

    private NodeView FindNodeview(core.graphs.Node node) 
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }
}
