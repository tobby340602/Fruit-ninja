using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LeastSquares.Razor
{

    /// <summary>
    /// A window that provides a guide to get started with Undertone.
    /// </summary>
    public class GettingStartedWindow : EditorWindow
    {
        private const string AssetName = InstallationWindowLoader.AssetName;
        private const string ShowOnStartPrefKey = AssetName + "ShowOnStart";
        private static bool _showOnStart = true;
        private Vector2 _scrollPosition;

        /// <summary>
        /// Adds a menu item to open the Undertone Getting Started window.
        /// </summary>
        [MenuItem("Window/" + AssetName + "/Getting Started")]
        public static void ShowWindow()
        {
            _showOnStart = EditorPrefs.GetBool(ShowOnStartPrefKey, true);
            if (_showOnStart)
                GetWindow<GettingStartedWindow>($"{AssetName} Getting Started");
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginVertical("Box");
            {
                var i = 1;
                RenderWelcomeLabel();
                RenderDemoScenesSection(ref i);
                RenderDocsAndLinks(ref i);
                RenderStartupToggle();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Renders the welcome label at the top of the window.
        /// </summary>
        private void RenderWelcomeLabel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Getting Started with {AssetName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// Renders the section for checking out the demo scenes.
        /// </summary>
        private void RenderDemoScenesSection(ref int Index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField($"Step {Index++}: Check out the demo scenes", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The following demo scenes are available for you to try:",
                    MessageType.Info);
                for (int i = 0; i < DemoSceneNames.Length; i++)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(DemoSceneNames[i].Replace($"Assets/{AssetName}/", string.Empty), GUILayout.Height(25), GUILayout.ExpandWidth(true)))
                    {
                        EditorSceneManager.OpenScene(DemoSceneNames[i]);
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        

        /// <summary>
        /// Renders the section for checking out the documentation and links.
        /// </summary>
        private void RenderDocsAndLinks(ref int Index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField($"Step {Index++}: Check out the documentation & more", EditorStyles.boldLabel);
                if (GUILayout.Button("Open Documentation"))
                    Application.OpenURL("https://leastsquares.io/docs/unity/razor");
                if (GUILayout.Button("Join Discord Community"))
                    Application.OpenURL("https://discord.gg/DZpBsTYNPD");
                if (GUILayout.Button("Check Other Assets"))
                    Application.OpenURL("https://assetstore.unity.com/publishers/39777&aid=1100lw2Qf");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Renders the toggle for showing the window on startup.
        /// </summary>
        private void RenderStartupToggle()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Show this window on startup", GUILayout.Width(400));
                _showOnStart = EditorGUILayout.Toggle(_showOnStart);
                EditorPrefs.SetBool(ShowOnStartPrefKey, _showOnStart);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private static readonly string[] DemoSceneNames =
        {
            $"Assets/{AssetName}/Demos/Ninja.unity",
            $"Assets/{AssetName}/Demos/Limbs.unity",
            $"Assets/{AssetName}/Demos/Katana.unity",
        };
    }
}