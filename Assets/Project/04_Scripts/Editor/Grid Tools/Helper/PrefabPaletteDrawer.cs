using UnityEditor;
using UnityEngine;

public class PrefabPaletteDrawer
{
    public void DrawPrefabPalette(GameObject[] availablePrefabs, ref int selectedPrefabIndex, ref Vector2 scrollPosition)
    {
        if (availablePrefabs == null || availablePrefabs.Length == 0)
        {
            EditorGUILayout.HelpBox("No prefabs found in Assets/Prefabs/Cell", MessageType.Warning);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < availablePrefabs.Length; i++)
        {
            if (i % 3 == 0) EditorGUILayout.BeginHorizontal();
            
            DrawPrefabButton(availablePrefabs, i, ref selectedPrefabIndex);
            
            if (i % 3 == 2 || i == availablePrefabs.Length - 1)
                EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        if (selectedPrefabIndex >= 0 && selectedPrefabIndex < availablePrefabs.Length)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Selected: {availablePrefabs[selectedPrefabIndex].name}");
        }
    }
    
    private void DrawPrefabButton(GameObject[] availablePrefabs, int index, ref int selectedPrefabIndex)
    {
        GameObject prefab = availablePrefabs[index];
        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
        
        string buttonText = (index == selectedPrefabIndex) ? $"[{prefab.name}]" : prefab.name;
        GUIContent content = new GUIContent(preview, buttonText);
        
        if (GUILayout.Button(content, GUILayout.Width(80), GUILayout.Height(80)))
        {
            selectedPrefabIndex = index;
        }
    }
}