using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToolsEditorGrid : EditorWindow
{
    #region Variables

    private const string TERRAIN_ROOT_NAME = "[GridTerrain]";

    private float gridCellSize = 1f;
    private int gridLineCount = 100;
    private int floorCount = 0;

    private GameObject[] availablePrefabs;
    private int selectedPrefabIndex = -1;

    private Vector2 scrollPosition;

    private SO_TerrainData terrainData;
    private Transform terrainRoot;

    private bool placementMode = false;

    private GameObject gameObjectSelected;
    private bool snappingGameobjectSelected = false;

    private Vector3 lastKnownPosition;
    private float gridOpacity = 1f;

    #endregion

    #region Handlers

    private PrefabLoader prefabLoader = new PrefabLoader();
    private GridRenderer gridRenderer = new GridRenderer();
    private PrefabPaletteDrawer paletteDrawer = new PrefabPaletteDrawer();
    private PlacementHandler placementHandler = new PlacementHandler();
    private SnappingHandler snappingHandler = new SnappingHandler();

    #endregion

    #region Window Management

    [MenuItem("Tools/Grid Editor")]
    public static void ShowWindow()
    {
        GetWindow<ToolsEditorGrid>("Grid Editor");
    }

    private void OnEnable()
    {
        LoadPrefabs();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    #endregion

    #region GUI

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);
        terrainData = (SO_TerrainData)EditorGUILayout.ObjectField("Terrain Data", terrainData, typeof(SO_TerrainData), false);

        if (terrainData == null)
        {
            placementMode = false;
            EditorGUILayout.HelpBox("Assign a Terrain Data asset to enable placement.", MessageType.Info);
        }

        using (new EditorGUI.DisabledScope(terrainData == null))
        {
            DrawTerrainActions();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(terrainData == null))
        {
            placementMode = EditorGUILayout.Toggle("Placement Mode", placementMode);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        gridCellSize = EditorGUILayout.FloatField("Cell Size", gridCellSize);
        gridLineCount = EditorGUILayout.IntField("Grid Extent", gridLineCount);
        floorCount = EditorGUILayout.IntField("Grid Floor", floorCount);
        gridOpacity = EditorGUILayout.Slider("Grid Opacity", gridOpacity, 0f, 1f);

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(10);
        EditSelectedGameObject();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Refresh Prefabs", GUILayout.Height(25)))
        {
            LoadPrefabs();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Prefab Palette", EditorStyles.boldLabel);

        paletteDrawer.DrawPrefabPalette(availablePrefabs, ref selectedPrefabIndex, ref scrollPosition);
    }

    private void EditSelectedGameObject()
    {
        gameObjectSelected = Selection.activeGameObject;

        if (gameObjectSelected == null) return;

        snappingGameobjectSelected = EditorGUILayout.Toggle("Snapping Mode", snappingGameobjectSelected);

        if (GUILayout.Button("Rotate 90°", GUILayout.Height(25)))
        {
            gameObjectSelected.transform.Rotate(90, PuzzleHelpers.RotationAxis.Y);
        }
    }

    #endregion

    #region Scene GUI

    private void OnSceneGUI(SceneView sceneView)
    {
        gridRenderer.DrawGrid(gridCellSize, gridLineCount, floorCount, gridOpacity);

        if (placementMode && selectedPrefabIndex >= 0 && terrainData != null)
        {
            if (terrainRoot == null) terrainRoot = GetOrCreateTerrainRoot();

            if (terrainRoot.gameObject.activeSelf)
            {
                placementHandler.HandlePlacement(sceneView, terrainData, terrainRoot, availablePrefabs,
                                                selectedPrefabIndex, gridCellSize, floorCount);
            }
        }

        snappingHandler.SnappingGameObjectSelected(gameObjectSelected, snappingGameobjectSelected,
                                                   gridCellSize, ref lastKnownPosition);

        if (gameObjectSelected != null)
        {
            gridRenderer.DrawGridY(gridCellSize, gridLineCount, gameObjectSelected.transform.position, gridOpacity);
        }
    }

    #endregion

    #region Terrain Root

    private static Transform FindTerrainRoot()
    {
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == TERRAIN_ROOT_NAME) return go.transform;
        }

        return null;
    }

    private Transform GetOrCreateTerrainRoot()
    {
        Transform found = FindTerrainRoot();
        if (found != null) return found;

        GameObject created = new GameObject(TERRAIN_ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(created, "Create Terrain Root");
        return created.transform;
    }

    private void DrawTerrainActions()
    {
        Transform root = FindTerrainRoot();
        bool isBaked = root != null && !root.gameObject.activeSelf;

        if (isBaked)
        {
            if (GUILayout.Button("Unbake", GUILayout.Height(25)))
            {
                TerrainBuilder.Unbake(root);
            }

            return;
        }

        if (GUILayout.Button("Rebuild From Data", GUILayout.Height(25)))
        {
            terrainRoot = GetOrCreateTerrainRoot();
            TerrainBuilder.Rebuild(terrainData, terrainRoot, gridCellSize);
        }

        if (GUILayout.Button("Bake", GUILayout.Height(25)))
        {
            terrainRoot = GetOrCreateTerrainRoot();
            TerrainBuilder.Bake(terrainData, terrainRoot, gridCellSize);
        }
    }

    #endregion

    #region Prefab Loading

    private void LoadPrefabs()
    {
        availablePrefabs = prefabLoader.LoadPrefabs();

        if (selectedPrefabIndex >= availablePrefabs.Length)
        {
            selectedPrefabIndex = -1;
        }
    }

    #endregion

    #region Selection

    private void OnSelectionChange()
    {
        snappingHandler.ResetLastKnownPosition(ref lastKnownPosition);
        placementMode = false;
        Repaint();
    }

    #endregion
}