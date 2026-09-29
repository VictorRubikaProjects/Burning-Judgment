using Grid;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TerrainBuilder
{
    private const string GENERATED_FOLDER = "Assets/Project/02_Prefabs/Tool/Terrain";
    private const string BAKED_SUFFIX = "_Baked";

    public static void Rebuild(SO_TerrainData data, Transform root, float gridCellSize)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
        }

        foreach (SO_TerrainData.Cell cell in data.Cells)
        {
            if (cell.prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cell.prefab, root);
            instance.transform.position = GridHelper.GridToWorld(cell.position, gridCellSize);
            Undo.RegisterCreatedObjectUndo(instance, "Rebuild Terrain");
        }

        Undo.CollapseUndoOperations(group);
    }

    public static GameObject Bake(SO_TerrainData data, Transform root, float gridCellSize)
    {
        Mesh mesh = TerrainMeshBuilder.Build(data, gridCellSize, out Material[] materials);

        if (mesh == null)
        {
            Debug.LogWarning("[TerrainBuilder] Nothing to bake: the terrain data has no valid cells.");
            return null;
        }

        DestroyBaked(root);
        EnsureFolder();

        string path = $"{GENERATED_FOLDER}/{data.name}{BAKED_SUFFIX}.asset";
        AssetDatabase.DeleteAsset(path);

        mesh.name = data.name + BAKED_SUFFIX;
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        GameObject baked = new GameObject(root.name + BAKED_SUFFIX);
        baked.AddComponent<MeshFilter>().sharedMesh = mesh;
        baked.AddComponent<MeshRenderer>().sharedMaterials = materials;
        baked.AddComponent<MeshCollider>().sharedMesh = mesh;
        Undo.RegisterCreatedObjectUndo(baked, "Bake Terrain");

        Undo.RecordObject(root.gameObject, "Bake Terrain");
        root.gameObject.SetActive(false);

        return baked;
    }

    public static void Unbake(Transform root)
    {
        DestroyBaked(root);

        Undo.RecordObject(root.gameObject, "Unbake Terrain");
        root.gameObject.SetActive(true);
    }

    private static void DestroyBaked(Transform root)
    {
        string bakedName = root.name + BAKED_SUFFIX;

        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name != bakedName) continue;

            Undo.DestroyObjectImmediate(go);
            return;
        }
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(GENERATED_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }
    }
}