using UnityEngine;
using System.Collections;

namespace core.modules
{
    public class DebugManager : BaseModule
    {
        // TODO
        // Load layout and submenus from json file (load a default one from this package, or ignore and load one from the game logic if it exists)
        // Build the entire layout only once
        // Find a dynamic way to execute logic from the layout. Try to make this module as dynamic and reusable as possible
        // (ex. button has a string callback for "GameManager.NeedThisCall" and Debug Manager needs to parse that callback)
        // careful with callbacks for classes in inaccessible namespaces?

        /* public bool fullscreenBool = false;
        private Vector2 scrollPosition; */
        private Matrix4x4 currentMatrix;
        private bool m_isActive = false;

        public bool isActive
        {
            get
            {
                return m_isActive;
            }

            set 
            { 
                m_isActive = value;

                // Wont this interfere?
                if(m_isActive)
                    SetMatrix();
                else
                    ResetMatrix();
            }
        }
        
        // Default size of debug manager
        private Vector2 nativeSize = new Vector2(1280, 720);

        public override void onInitialize()
        {
            currentMatrix = GUI.matrix;
        }

        private void DrawDebugMenu()
        {
            if(!isActive)
                return;
            
            /* GUILayout.BeginArea(new Rect(0, 0, 400, 600), "", "box");
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

            GUILayout.EndArea(); */
        }

        private void SetMatrix()
        {
            Vector3 scale = new Vector3 (Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, scale);
        }

        private void ResetMatrix()
        {
            GUI.matrix = currentMatrix;
        }

        public override void OnGUI()
        {
            DrawDebugMenu();
        }
    }
}