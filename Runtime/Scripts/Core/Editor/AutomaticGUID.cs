#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using core.gameplay;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutomaticGUID : Editor
{
    // TODO
    // Use the Editor Scene Manager event when closing a scene/saving a scene, to check and update all missing GUIDs
    // EditorSceneManager.sceneClosed and EditorSceneManager.sceneSaving

    [MenuItem("Save System/Generate All GUIDs", false, 1)]
    private static void GenerateGuid()
    {
        List<baseGameActor> actors = new List<baseGameActor>(FindObjectsOfType<baseGameActor>());
        int dirtyCounter = 0;

        foreach (baseGameActor o in actors)
        {
            if (!o.saveData.Enabled)
                continue;

            o.saveData.GenerateGUID();
            EditorUtility.SetDirty(o);
            EditorSceneManager.MarkSceneDirty(o.gameObject.scene);

            dirtyCounter++;
        }

        List<string> conflicts = actors.GroupBy(x => x.saveData.GUID).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

        if (conflicts.Count() > 0)
            Debug.LogError(string.Format(">> FAILED to generate GUIDs. {0} repeated GUIDs on GameActors!", conflicts.Count()));
        else if (dirtyCounter > 0)
            Debug.LogWarning(string.Format(">> New GUIDs generated for {0} GameActors!", dirtyCounter));
    }

    [MenuItem("Save System/Generate GUIDs For Selected", false, 2)]
    private static void GenerateGuidForSelected()
    {
        List<baseGameActor> actors = new List<baseGameActor>();

        foreach (Transform t in Selection.transforms)
        {
            baseGameActor actor = t.GetComponent<baseGameActor>();

            if (actor)
                actors.Add(actor);
        }

        int dirtyCounter = 0;

        foreach (baseGameActor o in actors)
        {
            if (!o.saveData.Enabled)
                continue;

            o.saveData.GenerateGUID();
            EditorUtility.SetDirty(o);
            EditorSceneManager.MarkSceneDirty(o.gameObject.scene);

            dirtyCounter++;
        }

        // Find conflicts in all that we can fetch
        actors = FindObjectsOfType<baseGameActor>().ToList();

        List<string> conflicts = actors.GroupBy(x => x.saveData.GUID).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

        if (conflicts.Count() > 0)
            Debug.LogError(string.Format(">> FAILED to generate GUIDs. {0} repeated GUIDs on GameActors!", conflicts.Count()));
        else if (dirtyCounter > 0)
            Debug.LogWarning(string.Format(">> New GUIDs generated for {0} GameActors!", dirtyCounter));
    }

    [MenuItem("Save System/Clear All GUIDs", false, 3)]
    private static void ClearGuid()
    {
        List<baseGameActor> actors = new List<baseGameActor>(FindObjectsOfType<baseGameActor>());
        int dirtyCounter = 0;

        foreach (baseGameActor o in actors)
        {
            if (!o.saveData.Enabled)
                continue;

            o.saveData.GUID = "";
            EditorUtility.SetDirty(o);
            EditorSceneManager.MarkSceneDirty(o.gameObject.scene);

            dirtyCounter++;
        }

        Debug.LogWarning(string.Format(">> New GUIDs generated for {0} GameActors!", dirtyCounter));
    }

    [MenuItem("Save System/Generate GUIDs For All Scenes")]
    static void UpdateGUIDAllScenes()
    {
        var changes = 0;
        var originalScene = EditorSceneManager.GetActiveScene().path;
        var sceneCounter = 0;

        foreach (var sceneGUID in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
        {
            sceneCounter++;
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);

            EditorSceneManager.OpenScene(scenePath);
            var scene = EditorSceneManager.GetActiveScene();

            var hasChanges = false;

            Action<GameObject> fix = null;

            fix = (obj) =>
            {
                baseGameActor _actor = obj.GetComponent<baseGameActor>();

                // If was successful
                if (_actor)
                {
                    // Generate GUID
                    _actor.saveData.GenerateGUID();
                    EditorUtility.SetDirty(obj);
                    ++changes;
                    hasChanges = true;
                    Debug.Log(scenePath + " ... " + obj);
                }

                // Check and update children -- maybe too intense
                for (var i = obj.transform.childCount - 1; i != -1; --i)
                    fix(obj.transform.GetChild(i).gameObject);
            };

            // Get all scene root objects and update their GUIDs 
            foreach (var obj in scene.GetRootGameObjects())
            {
                fix(obj);
            }

            if (hasChanges) EditorSceneManager.SaveScene(scene);
        }

        if (originalScene != "") EditorSceneManager.OpenScene(originalScene);
        
        Debug.LogWarning(string.Format(">> New GUIDs generated for {0} GameActors in {1} scenes!", changes, changes > 0 ? sceneCounter : 0));
    }
}
#endif