using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    [InitializeOnLoad]
    static class MainWindowInitializer
    {
        const string PrefKey = "Unity-MCP.MainWindow.Initialized";

        static bool isInitialized
        {
            get => PlayerPrefs.GetInt(PrefKey, 0) == 1;
            set => PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
        }

        static MainWindowInitializer()
        {
            if (isInitialized)
                return;

            EditorApplication.update += OnEditorUpdate;
        }

        static void OnEditorUpdate()
        {
            // Remove the callback to prevent repeated execution
            EditorApplication.update -= OnEditorUpdate;

            if (isInitialized)
                return;

            // Perform initialization
            McpPluginUnity.Init();
            MainWindowEditor.ShowWindow();

            isInitialized = true;
        }
    }
}