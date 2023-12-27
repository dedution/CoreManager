using UnityEngine;
using System.Collections;

namespace core.modules
{
    public class DebugManager : BaseModule
    {
        public bool fullscreenBool = false;

        public DebugManager() : base()
        {
        }

        private void DrawMenuTest()
        {
            GUILayout.BeginArea(new Rect(350, 90, 400, 300), "", "box");
            GUILayout.Box("Settings:");
            GUILayout.Space(5);
            GUILayout.Box("Sound:");
            GUILayout.Label("Volume:");
            GUILayout.BeginHorizontal();
            AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0.0f, 1.0f);
            GUILayout.Label("" + AudioListener.volume.ToString("0.0"), "labelSound", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.Box("Grahpics:");
            GUILayout.Space(500); // test scroll
            fullscreenBool = GUILayout.Toggle(fullscreenBool, "FullScreen?");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void OnGUI()
        {
            DrawMenuTest();
        }

        public void Hellow()
        {
            Debug.Log("Hellow from Debug Manager.");
        }
    }
}