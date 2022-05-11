using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Responsible for customising the LevelManager inspector window to allow level generation via the inspector.
/// </summary>
[CustomEditor(typeof(LevelManager))]
public class LevelEditor : Editor
{
    // the style of the heading to be used throughout the custom inspector
    private GUIStyle headingStyle;

    /*
     * These variables are used to hold the values given from the editors options.
     */

    // values from terrain options
    private int terrainSize;
    private int terrainType;
    private bool exactHeightEnabled;
    private bool heightRangeEnabled;
    private int exactHeightValue;
    private int minHeight;
    private int maxHeight;
    private int terrainShape;

    // values from waterbodies options - river
    private bool riverGenerationEnabled;
    private int numRiver;
    private bool riverIntersectionsEnabled;

    // values from waterbodies options - lake
    private bool lakeGenerationEnabled;
    private int numLake;
    private int maxLakeSize;

    // values from walkpath options
    private bool walkpathGenerationEnabled;
    private int numWalkpath;
    private bool walkpathIntersectionsEnabled;

    // state of the generation buttons (pressed or not pressed)
    private bool randomGenerationPressed;
    private bool levelGenerationPressed;

    // holds a reference to the levelManager object
    private LevelManager levelManager;

    /*
     * CONTINUE HEREEEEEEEE
     *
     */

    string levelGenInfo;
    Vector2 scrollPos = new Vector2();

    private void OnEnable()
    {
            levelManager = (LevelManager)target;
            levelManager.setupLevelFromInspector();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        headingStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

        addHeading("Level Generation Options", true);

        addTerrainOptions();

        addWaterBodiesOptions();

        addWalkpathOptions();

        addGenerationButtons();

        addLevelGenInfoSection();

        checkGenerationButtonPressed();
    }

    private void addTerrainOptions()
    {
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
    }

    private void addWaterBodiesOptions()
    {
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
    }

    private void addWalkpathOptions()
    {
        addHeading("Walkpath Options");

        walkpathGenerationEnabled = GUILayout.Toggle(walkpathGenerationEnabled, "Walkpath Generation");

        if (walkpathGenerationEnabled)
        {
            string[] numWalkpaths = Enum.GetNames(typeof(WalkpathGenerator.NumberOfWalkpaths));
            numWalkpath = EditorGUILayout.Popup("Number of Walkpaths", numWalkpath, numWalkpaths);

            walkpathIntersectionsEnabled = GUILayout.Toggle(walkpathIntersectionsEnabled, "Walkpath Intersection");
        }
    }

    private void addGenerationButtons()
    {
        EditorGUILayout.BeginHorizontal();
        randomGenerationPressed = GUILayout.Button("Randomise Level");
        levelGenerationPressed = GUILayout.Button("Generate Level");
        EditorGUILayout.EndHorizontal();
    }

    private void addLevelGenInfoSection()
    {
        addHeading("Level Generation Information");

        EditorGUILayout.BeginHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(150));
        GUILayout.Label(levelGenInfo);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void updateLevelGenInfo()
    {
        levelGenInfo = null;
        foreach (string generationStep in levelManager.getGenerationInfo())
        {
            levelGenInfo += generationStep + "\n";
        }
    }

    private void updateInspectorOptions(TerrainSettings terrainSettings, RiverSettings riverSettings, LakeSettings lakeSettings, WalkpathSettings walkpathSettings)
    {
        terrainSize = terrainSettings.tSize;
        terrainType = (int)terrainSettings.tType;

        heightRangeEnabled = terrainSettings.heightRangeEnabled;

        if (heightRangeEnabled)
        {
            minHeight = terrainSettings.tMinHeight;
            maxHeight = terrainSettings.tMaxHeight;
        }
        else
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
    }

    private void checkGenerationButtonPressed()
    {
        if (randomGenerationPressed || levelGenerationPressed)
        {
            TerrainSettings terrainSettings;
            RiverSettings riverSettings;
            LakeSettings lakeSettings;
            WalkpathSettings walkpathSettings;
            bool generateLevel;

            if (randomGenerationPressed)
            {
                terrainSettings = new TerrainSettings();
                riverSettings = new RiverSettings(terrainSettings.tType);
                lakeSettings = new LakeSettings(terrainSettings.tType);
                walkpathSettings = new WalkpathSettings(terrainSettings.tType);

                updateInspectorOptions(terrainSettings, riverSettings, lakeSettings, walkpathSettings);
                generateLevel = true;
            } else
            {
                terrainSettings = new TerrainSettings((TerrainGenerator.TerrainType)terrainType, heightRangeEnabled, terrainSize, (TerrainGenerator.TerrainShape)terrainShape, minHeight, maxHeight, exactHeightValue);
                riverSettings = new RiverSettings((TerrainGenerator.TerrainType)terrainType, riverGenerationEnabled, (RiverGenerator.NumberOfRivers)numRiver, riverIntersectionsEnabled);
                lakeSettings = new LakeSettings((TerrainGenerator.TerrainType)terrainType, lakeGenerationEnabled, (LakeGenerator.NumberOfLakes)numLake, (LakeGenerator.MaxLakeSize)maxLakeSize);
                walkpathSettings = new WalkpathSettings((TerrainGenerator.TerrainType)terrainType, walkpathGenerationEnabled, (WalkpathGenerator.NumberOfWalkpaths)numWalkpath, walkpathIntersectionsEnabled);

                if (terrainSettings.heightRangeIsOnAndInvalid())
                {
                    EditorUtility.DisplayDialog("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than or equal to the maximum value.", "Ok");
                    generateLevel = false;
                }
                else
                {
                    terrainSize = terrainSettings.tSize;
                    generateLevel = true;
                }
            }

            if (generateLevel)
            {
                levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

                updateLevelGenInfo();
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
