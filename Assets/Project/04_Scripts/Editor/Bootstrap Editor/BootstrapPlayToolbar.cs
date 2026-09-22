using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class BootstrapPlayToolbar
{
    private const string BootstrapScenePath = "Assets/Project/03_Scenes/Core/BootStrap.unity";

    static BootstrapPlayToolbar()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();

        if (EditorApplication.isPlaying) return;

        GUIStyle playStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.2f);

        if (GUILayout.Button(new GUIContent("▶ BootStrap"), playStyle, GUILayout.Width(95), GUILayout.Height(22)))
        {
            StartBootstrapPlay();
        }

        GUI.backgroundColor = Color.white;
    }

    private static void StartBootstrapPlay()
    {
        SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

        if (!bootstrapScene)
        {
            Debug.LogError($"[Bootstrap] Scène introuvable à l'emplacement : {BootstrapScenePath}");
            return;
        }

        EditorSceneManager.playModeStartScene = bootstrapScene;
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.playModeStartScene = null; 
        }
    }
}