using UnityEditor;

namespace LeastSquares.Razor
{
    [InitializeOnLoad]
    public class InstallationWindowLoader
    {
        public const string AssetName = "Razor";
        private const string ProjectOpenedKey = AssetName + "ProjectOpened";

        static InstallationWindowLoader()
        {
            EditorApplication.delayCall += ShowCustomMenuWindow;
        }

        private static void ShowCustomMenuWindow()
        {
            var value = EditorPrefs.GetBool(ProjectOpenedKey);
            if (!value)
            {
                GettingStartedWindow.ShowWindow();
                EditorPrefs.SetBool(ProjectOpenedKey, true);
            }
        }
    }
}