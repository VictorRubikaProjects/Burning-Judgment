using UnityEngine;

namespace Grid
{
    public static class GridHelper
    {
        #region Conversion

        public static Vector3Int WorldToGrid(Vector3 worldPosition, float cellSize)
        {
            Vector3Int gridPosition = new Vector3Int
            (
                Mathf.FloorToInt(worldPosition.x / cellSize),
                Mathf.FloorToInt(worldPosition.y / cellSize),
                Mathf.FloorToInt(worldPosition.z / cellSize)
            );
            
            return gridPosition;
        }

        public static Vector3 GridToWorld(Vector3Int gridPosition, float cellSize)
        {
            Vector3 worldPosition = new Vector3
            (
                (gridPosition.x * cellSize) + (cellSize * 0.5f),
                (gridPosition.y * cellSize) /* + (cellSize * 0.5f)*/ ,
                (gridPosition.z * cellSize) + (cellSize * 0.5f)
            );
            
            return worldPosition;
        }

        #endregion
    }
}