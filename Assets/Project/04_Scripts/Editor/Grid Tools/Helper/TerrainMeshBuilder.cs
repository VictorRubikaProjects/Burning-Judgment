using System.Collections.Generic;
using Grid;
using UnityEngine;
using UnityEngine.Rendering;

public static class TerrainMeshBuilder
{
    private const int CUBE_VERTEX_COUNT = 24;

    private struct Face
    {
        public Vector3Int normal;
        public Vector3 u;
        public Vector3 v;
    }

    private static readonly Face[] FACES =
    {
        new Face { normal = Vector3Int.right, u = Vector3.forward, v = Vector3.up },
        new Face { normal = Vector3Int.left, u = Vector3.up, v = Vector3.forward },
        new Face { normal = Vector3Int.up, u = Vector3.right, v = Vector3.forward },
        new Face { normal = Vector3Int.down, u = Vector3.forward, v = Vector3.right },
        new Face { normal = new Vector3Int(0, 0, 1), u = Vector3.up, v = Vector3.right },
        new Face { normal = new Vector3Int(0, 0, -1), u = Vector3.right, v = Vector3.up }
    };

    public static Mesh Build(SO_TerrainData data, float gridCellSize, out Material[] materials)
    {
        materials = null;

        List<Vector3> vertices = new();
        List<Vector3> normals = new();
        List<Vector2> uvs = new();
        Dictionary<Material, List<int>> trianglesByMaterial = new();
        HashSet<GameObject> warnedPrefabs = new();

        float half = gridCellSize * 0.5f;

        foreach (SO_TerrainData.Cell cell in data.Cells)
        {
            if (!TryGetCubeInfo(cell.prefab, warnedPrefabs, out Material material, out Vector3 offset)) continue;

            if (!trianglesByMaterial.TryGetValue(material, out List<int> triangles))
            {
                triangles = new List<int>();
                trianglesByMaterial[material] = triangles;
            }

            Vector3 center = GridHelper.GridToWorld(cell.position, gridCellSize) + offset;

            foreach (Face face in FACES)
            {
                if (data.IsOccupied(cell.position + face.normal)) continue;

                AddFace(face, center, half, vertices, normals, uvs, triangles);
            }
        }

        if (vertices.Count == 0) return null;

        List<Material> materialList = new List<Material>(trianglesByMaterial.Keys);

        Mesh mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = materialList.Count;

        for (int i = 0; i < materialList.Count; i++)
        {
            mesh.SetTriangles(trianglesByMaterial[materialList[i]], i);
        }

        mesh.RecalculateBounds();
        materials = materialList.ToArray();

        return mesh;
    }

    private static void AddFace(Face face, Vector3 center, float half, List<Vector3> vertices,
                                List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        Vector3 faceCenter = center + (Vector3)face.normal * half;
        Vector3 u = face.u * half;
        Vector3 v = face.v * half;
        int start = vertices.Count;

        vertices.Add(faceCenter - u - v);
        vertices.Add(faceCenter - u + v);
        vertices.Add(faceCenter + u + v);
        vertices.Add(faceCenter + u - v);

        for (int i = 0; i < 4; i++) normals.Add(face.normal);

        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(1f, 0f));

        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
    }

    private static bool TryGetCubeInfo(GameObject prefab, HashSet<GameObject> warnedPrefabs,
                                       out Material material, out Vector3 offset)
    {
        material = null;
        offset = Vector3.zero;

        if (prefab == null) return false;

        MeshFilter filter = prefab.GetComponentInChildren<MeshFilter>();
        MeshRenderer renderer = prefab.GetComponentInChildren<MeshRenderer>();

        if (filter == null || filter.sharedMesh == null || renderer == null || renderer.sharedMaterial == null)
        {
            WarnOnce(prefab, warnedPrefabs, "has no MeshFilter/MeshRenderer/Material and is skipped.");
            return false;
        }

        // TODO: support non-cube prefabs (ramps, corners, props) by combining their own meshes
        // instead of generating quads, and only cull the faces of standard cubes.
        if (filter.sharedMesh.vertexCount != CUBE_VERTEX_COUNT)
        {
            WarnOnce(prefab, warnedPrefabs, "is not a standard cube mesh. It is baked as a cube, its real shape is ignored.");
        }

        material = renderer.sharedMaterial;
        offset = Vector3.Scale(filter.sharedMesh.bounds.center, filter.transform.lossyScale);
        return true;
    }

    private static void WarnOnce(GameObject prefab, HashSet<GameObject> warnedPrefabs, string message)
    {
        if (!warnedPrefabs.Add(prefab)) return;

        Debug.LogWarning($"[TerrainMeshBuilder] Prefab '{prefab.name}' {message}", prefab);
    }
}