using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class GridEditorToolbar
{
    static GridEditorToolbar()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    private static void OnToolbarGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.9f);

        if (GUILayout.Button(new GUIContent("Grid Editor", "Open the Grid Editor"), style, GUILayout.Width(95), GUILayout.Height(22)))
        {
            ToolsEditorGrid.ShowWindow();
        }

        GUI.backgroundColor = Color.white;
    }
}