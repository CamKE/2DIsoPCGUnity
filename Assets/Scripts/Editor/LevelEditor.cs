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

    bool randomGenerationPressed;
    bool levelGenerationPressed;

    LevelManager levelManager;

    TerrainOptions terrainOptions;
    RiverOptions riverOptions;
    LakeOptions lakeOptions;
    WalkpathPathOptions walkpathPathOptions;

    string levelGenInfo;
    Vector2 scrollPos = new Vector2();

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
        randomGenerationPressed = GUILayout.Button("Randomise Level");
        levelGenerationPressed = GUILayout.Button("Generate Level");
        EditorGUILayout.EndHorizontal();

        addHeading("Level Generation Information");

        EditorGUILayout.BeginHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(150));
        GUILayout.Label(levelGenInfo);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        if (randomGenerationPressed)
        {
            TerrainOptions.TerrainSettings terrainSettings = terrainOptions.createRandomisedSettings();
            RiverOptions.RiverSettings riverSettings = riverOptions.createRandomisedSettings();
            LakeOptions.LakeSettings lakeSettings = lakeOptions.createRandomisedSettings();
            WalkpathPathOptions.WalkpathSettings walkpathSettings = walkpathPathOptions.createRandomisedSettings();

            levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

            terrainSize = terrainSettings.tSize;
            terrainType = (int)terrainSettings.tType;
            heightRangeEnabled = terrainSettings.heightRangedEnabled;

            if (heightRangeEnabled)
            {
                minHeight = terrainSettings.tMinHeight;
                maxHeight = terrainSettings.tMaxHeight;
            } else
            {
                exactHeightValue = terrainSettings.tExactHeight;
            }

            terrainShape = (int)terrainSettings.tShape;

            riverGenerationEnabled = riverSettings.rGenerationEnabled;

            if (riverGenerationEnabled)
            {
                numRiver = (int)riverSettings.rNum;
                riverIntersectionsEnabled = riverSettings.intersectionsEnabled;
            }

            lakeGenerationEnabled = lakeSettings.lGenerationEnabled;

            if (lakeGenerationEnabled)
            {
                numLake = (int)lakeSettings.lNum;
                maxLakeSize = (int)lakeSettings.lMaxSize;
            }

            walkpathGenerationEnabled = walkpathSettings.wGenerationEnabled;

            if (walkpathGenerationEnabled)
            {
                numWalkpath = (int)walkpathSettings.wNum;
                walkpathIntersectionsEnabled = walkpathSettings.intersectionsEnabled;
            }

            foreach (string generationStep in levelManager.getGenerationInfo())
            {
                levelGenInfo += generationStep + "\n";
            }
        }

        if (levelGenerationPressed)
        {
            TerrainOptions.TerrainSettings terrainSettings = terrainOptions.createUserSettings(terrainType, heightRangeEnabled, terrainSize, terrainShape, minHeight, maxHeight, exactHeightValue);

            if (terrainSettings.heightRangeIsOnAndInvalid())
            {
                EditorUtility.DisplayDialog("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than or equal to the maximum value.", "Ok");
            }
            else
            {
                RiverOptions.RiverSettings riverSettings = riverOptions.createUserSettings(riverGenerationEnabled, numRiver, riverIntersectionsEnabled);
                LakeOptions.LakeSettings lakeSettings = lakeOptions.createUserSettings(lakeGenerationEnabled, numLake, maxLakeSize);
                WalkpathPathOptions.WalkpathSettings walkpathSettings = walkpathPathOptions.createUserSettings(walkpathGenerationEnabled, numWalkpath, walkpathIntersectionsEnabled);

                levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);
            }

            foreach (string generationStep in levelManager.getGenerationInfo())
            {
                levelGenInfo += generationStep + "\n";
            }
        }
    }

    private void addHeading(string heading, bool mainHeading = false)
    {
        EditorGUILayout.Space();
        headingStyle.fontSize = mainHeading ? 15 : 13;
        EditorGUILayout.LabelField(heading, headingStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }


}
