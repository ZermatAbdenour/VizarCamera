using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    [InitializeOnLoad]
    public class TriLibSplashScreen
    {
        static TriLibSplashScreen()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            EditorApplication.update -= Update;
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorPrefs.GetBool(TriLibVersionNotes.SkipVersionInfoKey))
            {
                TriLibVersionNotes.ShowWindow();
            }
        }
    }
}