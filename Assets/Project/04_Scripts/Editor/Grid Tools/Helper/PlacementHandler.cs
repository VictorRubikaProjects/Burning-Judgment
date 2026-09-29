using Grid;
using UnityEditor;
using UnityEngine;

public class PlacementHandler
{
    private const float MATCH_TOLERANCE = 0.01f;

    private Vector3Int previewGridPosition;
    private bool isValidPlacement;
    private bool isRemoving;

    public void HandlePlacement(SceneView sceneView, SO_TerrainData terrainData, Transform terrainRoot,
                                GameObject[] availablePrefabs, int selectedPrefabIndex,
                                float gridCellSize, int floorCount)
    {
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, floorCount * gridCellSize, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            Vector3Int flatPosition = GridHelper.WorldToGrid(worldPos, gridCellSize);
            previewGridPosition = new Vector3Int(flatPosition.x, floorCount, flatPosition.z);

            bool occupied = terrainData.IsOccupied(previewGridPosition);
            isRemoving = e.shift;
            isValidPlacement = isRemoving ? occupied : !occupied;

            DrawPlacementPreview(gridCellSize);

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && isValidPlacement)
            {
                if (isRemoving)
                {
                    RemoveCell(terrainData, terrainRoot, gridCellSize);
                }
                else if (selectedPrefabIndex >= 0 && selectedPrefabIndex < availablePrefabs.Length)
                {
                    PlaceCell(terrainData, terrainRoot, availablePrefabs[selectedPrefabIndex], gridCellSize);
                }

                e.Use();
            }

            sceneView.Repaint();
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void DrawPlacementPreview(float gridCellSize)
    {
        Vector3 worldPos = GridHelper.GridToWorld(previewGridPosition, gridCellSize);

        if (!isValidPlacement) Handles.color = new Color(1f, 0f, 0f, 0.1f);
        else if (isRemoving) Handles.color = new Color(1f, 0.9f, 0f, 0.1f);
        else Handles.color = new Color(0f, 1f, 0f, 0.1f);

        Handles.CubeHandleCap(0, worldPos, Quaternion.identity, gridCellSize * 0.9f, EventType.Repaint);
    }

    private void PlaceCell(SO_TerrainData terrainData, Transform terrainRoot, GameObject prefab, float gridCellSize)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(terrainData, "Place Cell");
        terrainData.SetCell(previewGridPosition, prefab);
        EditorUtility.SetDirty(terrainData);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, terrainRoot);
        instance.transform.position = GridHelper.GridToWorld(previewGridPosition, gridCellSize);
        Undo.RegisterCreatedObjectUndo(instance, "Place Cell");

        Undo.CollapseUndoOperations(group);
    }

    private void RemoveCell(SO_TerrainData terrainData, Transform terrainRoot, float gridCellSize)
    {
        Vector3 target = GridHelper.GridToWorld(previewGridPosition, gridCellSize);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(terrainData, "Remove Cell");
        terrainData.RemoveCell(previewGridPosition);
        EditorUtility.SetDirty(terrainData);

        for (int i = terrainRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = terrainRoot.GetChild(i);
            if (Vector3.Distance(child.position, target) > MATCH_TOLERANCE) continue;

            Undo.DestroyObjectImmediate(child.gameObject);
            break;
        }

        Undo.CollapseUndoOperations(group);
    }
}