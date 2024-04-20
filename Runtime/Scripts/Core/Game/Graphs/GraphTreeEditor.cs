using core.graphs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphTreeEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private GraphTreeView treeView;
    private InspectorView inspectorView;


    [MenuItem("Graphs/AI Behavior Editor")]
    public static void ShowExample()
    {
        GraphTreeEditor wnd = GetWindow<GraphTreeEditor>();
        wnd.titleContent = new GUIContent("GraphTreeEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        m_VisualTreeAsset.CloneTree(root);

        treeView = root.Q<GraphTreeView>();
        inspectorView = root.Q<InspectorView>();
    }

    // THIS WILL BE REMOVED.
    // Having an option to open instead may lead to less bugs in the long run
    private void OnSelectionChange() {
        GraphTree tree = Selection.activeObject as GraphTree;

        if(tree)
        {
            treeView.PopulateView(tree);
        }
    }
}
