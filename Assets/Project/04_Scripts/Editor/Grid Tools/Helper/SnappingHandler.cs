using Grid;
using UnityEditor;
using UnityEngine;

public class SnappingHandler
{
    private Vector3 lastKnownPosition;

    public void SnappingGameObjectSelected(GameObject gameObjectSelected, bool snappingGameobjectSelected, 
        float gridCellSize, ref Vector3 lastKnownPositionRef)
    {
        if (!snappingGameobjectSelected || gameObjectSelected == null) return;

        Vector3 actualPositionSelectedObject = gameObjectSelected.transform.position;

        if (lastKnownPositionRef == Vector3.zero)
        {
            lastKnownPositionRef = actualPositionSelectedObject;
            return; 
        }
        
        if (Vector3.Distance(actualPositionSelectedObject, lastKnownPositionRef) > 0.01f)
        {
            Vector3Int gridPosition = GridHelper.WorldToGrid(actualPositionSelectedObject, gridCellSize);
            Vector3 newPosition = GridHelper.GridToWorld(gridPosition, gridCellSize);
            Undo.RecordObject(gameObjectSelected.transform, "Snap to Grid");
            gameObjectSelected.transform.position = newPosition;
            lastKnownPositionRef = newPosition;
        }
    }
    
    public void ResetLastKnownPosition(ref Vector3 lastKnownPositionRef)
    {
        lastKnownPositionRef = Vector3.zero;
    }
}