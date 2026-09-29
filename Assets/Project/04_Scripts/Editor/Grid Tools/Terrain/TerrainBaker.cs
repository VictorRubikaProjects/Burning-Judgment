using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class TerrainBaker
{
    public static GameObject Bake(GameObject root, string assetPath)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
        Dictionary<Material, List<CombineInstance>> byMaterial = new Dictionary<Material, List<CombineInstance>>();

        foreach (MeshFilter filter in filters)
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.sharedMaterials;
            for (int sub = 0; sub < filter.sharedMesh.subMeshCount && sub < materials.Length; sub++)
            {
                if (!byMaterial.TryGetValue(materials[sub], out List<CombineInstance> list))
                {
                    list = new List<CombineInstance>();
                    byMaterial[materials[sub]] = list;
                }

                list.Add(new CombineInstance
                {
                    mesh = filter.sharedMesh,
                    subMeshIndex = sub,
                    transform = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix
                });
            }
        }

        List<Mesh> perMaterial = new List<Mesh>();
        List<Material> orderedMaterials = new List<Material>();
        List<CombineInstance> final = new List<CombineInstance>();

        foreach (KeyValuePair<Material, List<CombineInstance>> pair in byMaterial)
        {
            Mesh part = new Mesh { indexFormat = IndexFormat.UInt32 };
            part.CombineMeshes(pair.Value.ToArray(), true, true);
            perMaterial.Add(part);
            orderedMaterials.Add(pair.Key);
            final.Add(new CombineInstance { mesh = part, transform = Matrix4x4.identity });
        }

        Mesh baked = new Mesh { name = root.name + "_Baked", indexFormat = IndexFormat.UInt32 };
        baked.CombineMeshes(final.ToArray(), false, false);
        baked.RecalculateBounds();

        AssetDatabase.CreateAsset(baked, assetPath);

        GameObject result = new GameObject(root.name + "_Baked");
        result.transform.SetPositionAndRotation(root.transform.position, root.transform.rotation);
        result.AddComponent<MeshFilter>().sharedMesh = baked;
        result.AddComponent<MeshRenderer>().sharedMaterials = orderedMaterials.ToArray();
        result.AddComponent<MeshCollider>().sharedMesh = baked;

        Undo.RegisterCreatedObjectUndo(result, "Bake Terrain");
        return result;
    }
}