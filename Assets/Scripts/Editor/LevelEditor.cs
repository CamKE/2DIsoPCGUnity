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

    // holds the text for the level generation information
    private string levelGenInfo;
    // scroll position for the level gen info scroll view
    Vector2 levelGenInfoScrollPos = new Vector2();

    // called when the script is activated (when object is clicked in inspector. Object is 
    // deactivated when not clicked)
    private void OnEnable()
    {
            // get reference to LevelManage object
            levelManager = (LevelManager)target;
            // setup the level
            levelManager.setupLevelFromInspector();
    }

    /// <summary>
    /// Function from built in editor class. Implemented to make the custom inspector, adding custom
    /// GUI elements.
    /// </summary>
    // this function is looped
    public override void OnInspectorGUI()
    {
        // draw the level manager default inspector first
        DrawDefaultInspector();
        // create a new style for heading
        headingStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        // create main heading
        addHeading("Level Generation Options", true);
        // add the options
        addTerrainOptions();
        addWaterBodiesOptions();
        addWalkpathOptions();
        // add the buttons
        addGenerationButtons();
        // add the level gen information
        addLevelGenInfoSection();
        // check the status of the buttons
        checkGenerationButtonPressed();
    }

    // responsible for adding all terrain generation options
    private void addTerrainOptions()
    {
        // terrain options sub heading
        addHeading("Terrain Options");

        // add slider for terrain size
        terrainSize = EditorGUILayout.IntSlider("Terrain Size", terrainSize, TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);

        // dropdown for terrain type
        string[] terrainTypes = Enum.GetNames(typeof(TerrainGenerator.TerrainType));
        terrainType = EditorGUILayout.Popup("Terrain Type", terrainType, terrainTypes);

        // toggles for terrain height. only one can be active at a time. horizontal layout
        EditorGUILayout.BeginHorizontal();
        exactHeightEnabled = GUILayout.Toggle(!heightRangeEnabled, "Exact Height");
        heightRangeEnabled = GUILayout.Toggle(!exactHeightEnabled, "Height Range");
        EditorGUILayout.EndHorizontal();
        
        // checks which height toggle is on
        // for exact height
        if (exactHeightEnabled)
        {
            // show the exact height slider
            exactHeightValue = EditorGUILayout.IntSlider("Exact Height", exactHeightValue, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }
        else
        // otherwise for height range
        {
            // show the min and max height sliders
            minHeight = EditorGUILayout.IntSlider("Minimum Height", minHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
            maxHeight = EditorGUILayout.IntSlider("Maxiumum Height", maxHeight, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }

        // dropdown for terrain shape
        // get options based on terrainshape enum in terrain generator
        string[] terrainShapes = Enum.GetNames(typeof(TerrainGenerator.TerrainShape));
        terrainShape = EditorGUILayout.Popup("Terrain Shape", terrainShape, terrainShapes);
    }

    // responsible for adding all water body options
    private void addWaterBodiesOptions()
    {
        // sub heading
        addHeading("Water Bodies Options");

        // river gen enabled toggle
        riverGenerationEnabled = GUILayout.Toggle(riverGenerationEnabled, "River Generation");

        // if river gen enabled
        if (riverGenerationEnabled)
        {
            // show number of rivers dropdown
            string[] numRivers = Enum.GetNames(typeof(RiverGenerator.NumberOfRivers));
            numRiver = EditorGUILayout.Popup("Number of Rivers", numRiver, numRivers);

            // show intersection toggle
            riverIntersectionsEnabled = GUILayout.Toggle(riverIntersectionsEnabled, "River Intersection");
        }

        // lake gen enabled toggle
        lakeGenerationEnabled = GUILayout.Toggle(lakeGenerationEnabled, "Lake Generation");

        // if lake gen enabled
        if (lakeGenerationEnabled)
        {
            // show number of lakes dropdown
            string[] numLakes = Enum.GetNames(typeof(LakeGenerator.NumberOfLakes));
            numLake = EditorGUILayout.Popup("Number of Lakes", numLake, numLakes);

            // show max lake size dropdown
            string[] maxLakeSizes = Enum.GetNames(typeof(LakeGenerator.MaxLakeSize));
            maxLakeSize = EditorGUILayout.Popup("Maximum Lake Size", maxLakeSize, maxLakeSizes);
        }
    }

    // responsible for adding walkpath options
    private void addWalkpathOptions()
    {
        // sub heading
        addHeading("Walkpath Options");

        // enabled toggle
        walkpathGenerationEnabled = GUILayout.Toggle(walkpathGenerationEnabled, "Walkpath Generation");

        // if enabled
        if (walkpathGenerationEnabled)
        {
            // show dropdown
            string[] numWalkpaths = Enum.GetNames(typeof(WalkpathGenerator.NumberOfWalkpaths));
            numWalkpath = EditorGUILayout.Popup("Number of Walkpaths", numWalkpath, numWalkpaths);

            // show intersection toggle
            walkpathIntersectionsEnabled = GUILayout.Toggle(walkpathIntersectionsEnabled, "Walkpath Intersection");
        }
    }

    // adds the generation buttons to the custom inspector
    private void addGenerationButtons()
    {
        // add randomise level and generate level buttons. horizontal layout
        EditorGUILayout.BeginHorizontal();
        randomGenerationPressed = GUILayout.Button("Randomise Level");
        levelGenerationPressed = GUILayout.Button("Generate Level");
        EditorGUILayout.EndHorizontal();
    }

    // adds the level generation information section
    private void addLevelGenInfoSection()
    {
        // sub heading
        addHeading("Level Generation Information");

        levelGenInfoScrollPos = EditorGUILayout.BeginScrollView(levelGenInfoScrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(150));
        GUILayout.Label(levelGenInfo);
        EditorGUILayout.EndScrollView();
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
