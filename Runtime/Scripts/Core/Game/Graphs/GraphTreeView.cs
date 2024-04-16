using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<GraphTreeView, GraphView.UxmlTraits> { }

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
}
