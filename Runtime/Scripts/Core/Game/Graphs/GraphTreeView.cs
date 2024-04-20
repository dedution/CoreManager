using System;
using System.Collections;
using System.Collections.Generic;
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

    private void CreateNode(Type type)
    {
        core.graphs.Node node = tree.CreateNode(type);
        CreateNodeView(node);
    }


    public void PopulateView(GraphTree tree)
    {
        this.tree = tree;
        DeleteElements(graphElements);

        tree.nodes.ForEach(n => CreateNodeView(n));
    }

    private void CreateNodeView(core.graphs.Node node)
    {
        NodeView nodeView = new NodeView(node);
        AddElement(nodeView);
    }
}
