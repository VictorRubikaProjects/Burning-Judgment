using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Grid/Terrain Data", fileName = "TerrainData")]
public class SO_TerrainData : ScriptableObject,ISerializationCallbackReceiver
{
    [Serializable]
    public struct Cell
    {
        public Vector3Int position;
        public GameObject prefab;
    }

    [SerializeField] private List<Cell> cells = new List<Cell>();

    private Dictionary<Vector3Int, int> lookup;

    public IReadOnlyList<Cell> Cells => cells;

    private void OnEnable() => RebuildLookup();

    private void RebuildLookup()
    {
        lookup = new Dictionary<Vector3Int, int>(cells.Count);
        for (int i = 0; i < cells.Count; i++) lookup[cells[i].position] = i;
    }

    public bool IsOccupied(Vector3Int position)
    {
        if (lookup == null) RebuildLookup();
        return lookup.ContainsKey(position);
    }

    public void SetCell(Vector3Int position, GameObject prefab)
    {
        if (lookup == null) RebuildLookup();

        Cell cell = new Cell { position = position, prefab = prefab };
        if (lookup.TryGetValue(position, out int index))
        {
            cells[index] = cell;
            return;
        }

        lookup[position] = cells.Count;
        cells.Add(cell);
    }

    public bool RemoveCell(Vector3Int position)
    {
        if (lookup == null) RebuildLookup();
        if (!lookup.TryGetValue(position, out int index)) return false;

        int last = cells.Count - 1;
        cells[index] = cells[last];
        cells.RemoveAt(last);
        RebuildLookup();
        return true;
    }

    public void Clear()
    {
        cells.Clear();
        RebuildLookup();
    }
    
    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() => lookup = null;
}