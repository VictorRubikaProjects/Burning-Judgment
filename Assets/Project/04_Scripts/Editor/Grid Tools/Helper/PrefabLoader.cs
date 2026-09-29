using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabLoader
{
    private const string PREFABS_PATH = "Assets/Project/02_Prefabs/Tool/Input";
    
    public GameObject[] LoadPrefabs()
    {
        GameObject[] availablePrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { PREFABS_PATH })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/") == PREFABS_PATH)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            .Where(prefab => prefab != null)
            .ToArray();
        
        return availablePrefabs;
    }
}