#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

// Disable certain gameobjects before starting the build proccess. 
// This is a fail safe to disable gameobjects that are supposed to be activated over time
// class SceneOptimizer : IPreprocessBuildWithReport
// {
//     public int callbackOrder { get { return 0; } }
//     public void OnPreprocessBuild(BuildReport report)
//     {
//         // Recheck scenes and disable necessary gameobjects for timed activation
//         foreach (var sceneGUID in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
//         {
//             var scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);

//             EditorSceneManager.OpenScene(scenePath);
//             var scene = EditorSceneManager.GetActiveScene();

//             var hasChanges = false;

//             // Get all scene root objects and update their GUIDs 
//             foreach (var obj in scene.GetRootGameObjects())
//             {
//                 // Instead of every root object, disable the objects that contain behaviors with rough inits
//                 // Based on the work done on Evil Below, a specific tag would be suficient to identify the root object of the timed activations
//                 // Disable the children of the tagged gameobject
//                 if (obj.activeSelf)
//                 {
//                     obj.SetActive(false);
//                     hasChanges = true;
//                 }
//             }

//             if (hasChanges) EditorSceneManager.SaveScene(scene);
//         }
//     }
// }

// EVENTS OF CLOSING, OPENING AND SAVING SCENES
// GUID optimizations could be implemented here for example

[InitializeOnLoad]
class SceneEvents
{
    static SceneEvents()
    {
        // Events for scene opening and closing
        EditorSceneManager.sceneClosing += SceneClosing;
        EditorSceneManager.sceneClosed += SceneClosed;
        EditorSceneManager.sceneOpening += SceneOpening;
        EditorSceneManager.sceneOpened += SceneOpened;
        EditorSceneManager.newSceneCreated += NewSceneCreated;
    }

    static void SceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
    {
        //Debug.Log("SceneClosing");
    }

    static void SceneClosed(UnityEngine.SceneManagement.Scene scene)
    {
        //Debug.Log("SceneClosed");
    }

    static void SceneOpening(string path, OpenSceneMode mode)
    {
        //Debug.Log("SceneOpening");
    }

    static void SceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        //Debug.Log("SceneOpened");
    }

    static void NewSceneCreated(UnityEngine.SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode)
    {
        //Debug.Log("NewSceneCreated");
    }
}

#endif