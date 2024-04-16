using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphTreeEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/Graphs/AI Behavior Editor")]
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
    }
}
