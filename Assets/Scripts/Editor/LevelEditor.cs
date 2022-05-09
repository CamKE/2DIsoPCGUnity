using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

/// <summary>
/// Responsible for customising the inspector window to allow for easy level generation via the inspector.
/// </summary>
[CustomEditor(typeof(LevelManager))]
public class LevelEditor : Editor
{
    //EditorUtility.DisplayDialog("This is a test", "Test message body", "Close");
    GUIStyle headingStyle;

    int terrainSize;
    int terrainType;
    bool exactHeightEnabled;
    bool heightRangeEnabled;
    int exactHeightValue;
    int minHeight;
    int maxHeight;
    int terrainShape;

    bool riverGenerationEnabled;
    int numRiver;
    bool riverIntersectionsEnabled;

    bool lakeGenerationEnabled;
    int numLake;
    int maxLakeSize;

    bool walkpathGenerationEnabled;
    int numWalkpath;
    bool walkpathIntersectionsEnabled;

    LevelManager levelManager;

    TerrainOptions terrainOptions;
    RiverOptions riverOptions;
    LakeOptions lakeOptions;
    WalkpathPathOptions walkpathPathOptions;

    void Awake()
    {
        Debug.Log("hello");
    }

    private void OnEnable()
    {
            levelManager = (LevelManager)target;

            terrainOptions = new TerrainOptions();
            riverOptions = new RiverOptions();
            lakeOptions = new LakeOptions();
            walkpathPathOptions = new WalkpathPathOptions();

            levelManager.setupLevelFromInspector();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        headingStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

        addHeading("Level Generation Options", true);

        addHeading("Terrain Options");

        terrainSize = EditorGUILayout.IntSlider("Terrain Size", terrainSize, TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);
        
        string[] terrainTypes = Enum.GetNames(typeof(TerrainGenerator.TerrainType));
        terrainType = EditorGUILayout.Popup("Terrain Type", terrainType, terrainTypes);

        EditorGUILayout.BeginHorizontal();
        exactHeightEnabled = GUILayout.Toggle(!heightRangeEnabled, "Exact Height");
        heightRangeEnabled = GUILayout.Toggle(!exactHeightEnabled, "Height Range");
        EditorGUILayout.EndHorizontal();

        if (exactHeightEnabled)
        {
            exactHeightValue = EditorGUILayout.IntSlider("Exact Height", exactHeightValue, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }
        else
        {
            minHeight = EditorGUILayout.IntSlider("Minimum Height", minHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
            maxHeight = EditorGUILayout.IntSlider("Maxiumum Height", maxHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }

        string[] terrainShapes = Enum.GetNames(typeof(TerrainGenerator.TerrainShape));
        terrainShape = EditorGUILayout.Popup("Terrain Shape", terrainShape, terrainShapes);

        addHeading("Water Bodies Options");

        riverGenerationEnabled = GUILayout.Toggle(riverGenerationEnabled, "River Generation");

        if (riverGenerationEnabled)
        {
            string[] numRivers = Enum.GetNames(typeof(RiverGenerator.NumberOfRivers));
            numRiver = EditorGUILayout.Popup("Number of Rivers", numRiver, numRivers);

            riverIntersectionsEnabled = GUILayout.Toggle(riverIntersectionsEnabled, "River Intersection");
        }

        lakeGenerationEnabled = GUILayout.Toggle(lakeGenerationEnabled, "Lake Generation");

        if (lakeGenerationEnabled)
        {
            string[] numLakes = Enum.GetNames(typeof(LakeGenerator.NumberOfLakes));
            numLake = EditorGUILayout.Popup("Number of Lakes", numLake, numLakes);

            string[] maxLakeSizes = Enum.GetNames(typeof(LakeGenerator.MaxLakeSize));
            maxLakeSize = EditorGUILayout.Popup("Maximum Lake Size", maxLakeSize, maxLakeSizes);
        }

        addHeading("Walkpath Options");

        walkpathGenerationEnabled = GUILayout.Toggle(walkpathGenerationEnabled, "Walkpath Generation");

        if (walkpathGenerationEnabled)
        {
            string[] numWalkpaths = Enum.GetNames(typeof(WalkpathGenerator.NumberOfWalkpaths));
            numWalkpath = EditorGUILayout.Popup("Number of Walkpaths", numWalkpath, numWalkpaths);

            walkpathIntersectionsEnabled = GUILayout.Toggle(walkpathIntersectionsEnabled, "Walkpath Intersection");
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomise Level"))
        {
            TerrainOptions.TerrainSettings terrainSettings = terrainOptions.createRandomisedSettings();
            RiverOptions.RiverSettings riverSettings = riverOptions.createRandomisedSettings();
            LakeOptions.LakeSettings lakeSettings = lakeOptions.createRandomisedSettings();
            WalkpathPathOptions.WalkpathSettings walkpathSettings = walkpathPathOptions.createRandomisedSettings();

            levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);
        }

        if (GUILayout.Button("Generate Level"))
        {
            //levelManager.generate();
        }
        EditorGUILayout.EndHorizontal();

    }

    private void addHeading(string heading, bool mainHeading = false)
    {
        EditorGUILayout.Space();
        headingStyle.fontSize = mainHeading ? 15 : 13;
        EditorGUILayout.LabelField(heading, headingStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }


}
