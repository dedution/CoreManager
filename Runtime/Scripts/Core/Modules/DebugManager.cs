using UnityEngine;
using System.Collections;

namespace core.modules
{
    public class DebugManager : BaseModule
    {
        public bool fullscreenBool = false;
        private Vector2 scrollPosition;

        public override void onInitialize()
        {
            
        }

        private void DrawMenuTest()
        {
            GUILayout.BeginArea(new Rect(0, 0, 400, 600), "", "box");
            scrollPosition = GUILayout.BeginScrollView(
            scrollPosition, GUILayout.Width(400), GUILayout.Height(500));
            GUILayout.Box("Settings:");
            GUILayout.Space(5);
            GUILayout.Box("Sound:");
            GUILayout.Label("Volume:");
            GUILayout.BeginHorizontal();
            
            AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0.0f, 1.0f);
            GUILayout.Label("" + AudioListener.volume.ToString("0.0"), "labelSound", GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(500); // test scroll
            fullscreenBool = GUILayout.Toggle(fullscreenBool, "FullScreen?");
            GUILayout.EndScrollView();
            
            GUILayout.Space(25);
            GUILayout.Label("______________________________________________");
            GUILayout.Label("BUILD ID: XXXXXXX");
            GUILayout.Label("BUILD DATE: 01-01-0101");

            GUILayout.EndArea();
        }

        public override void OnGUI()
        {
            //DrawMenuTest();
        }

        public void Hellow()
        {
            Debug.Log("Hellow from Debug Manager.");
        }
    }
}