using UnityEditor;
using UnityEngine;

public class GridRenderer
{
    public void DrawGrid(float gridCellSize, int gridLineCount, int floorCount, float gridOpacity)
    {
        Handles.color = new Color(0.0f, 1f, 0f, gridOpacity);
        float extent = gridLineCount * gridCellSize;

        for (int i = -gridLineCount; i <= gridLineCount; i++)
        {
            float position = i * gridCellSize;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            
            Handles.DrawLine(new Vector3(position, floorCount * gridCellSize, -extent),
                new Vector3(position, floorCount * gridCellSize, extent));
            Handles.DrawLine(new Vector3(-extent, floorCount * gridCellSize, position),
                new Vector3(extent, floorCount * gridCellSize, position));
        }
    }

    public void DrawGridY(float gridCellSize, int gridLineCount, Vector3 positionGameObject, float gridOpacity)
    {
        Handles.color = new Color(1f, 0f, 0f, gridOpacity);
        float extent = gridLineCount * gridCellSize;

        float positionX = positionGameObject.x;
        float positionZ = positionGameObject.z;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            
        Handles.DrawLine(new Vector3(positionX, -extent, positionZ),
            new Vector3(positionX, extent, positionZ));
    }
}