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
    private bool randomiseLevelPressed;
    private bool generateLevelPressed;

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
        randomiseLevelPressed = GUILayout.Button("Randomise Level");
        generateLevelPressed = GUILayout.Button("Generate Level");
        EditorGUILayout.EndHorizontal();
    }

    // adds the level generation information section
    private void addLevelGenInfoSection()
    {
        // sub heading
        addHeading("Level Generation Information");

        // level gen scroll window
        levelGenInfoScrollPos = EditorGUILayout.BeginScrollView(levelGenInfoScrollPos, GUILayout.ExpandWidth(true), GUILayout.Height(150));
        GUILayout.Label(levelGenInfo);
        EditorGUILayout.EndScrollView();
    }

    // updates the levelGenInfo string. called after level generation
    private void updateLevelGenInfo()
    {
        // reset to null
        levelGenInfo = null;

        // concatenate the steps into a single string seperated by newlines
        foreach (string generationStep in levelManager.getGenerationInfo())
        {
            levelGenInfo += generationStep + "\n";
        }
    }

    // updates the options with the values used for random generation
    private void updateInspectorOptions(TerrainSettings terrainSettings, RiverSettings riverSettings, LakeSettings lakeSettings, WalkpathSettings walkpathSettings)
    {
        /*
         * Set terrain settings
         */
        terrainSize = terrainSettings.tSize;
        terrainType = (int)terrainSettings.tType;

        heightRangeEnabled = terrainSettings.heightRangeEnabled;

        // if height range is on
        if (heightRangeEnabled)
        {
            // set min and max height values used
            minHeight = terrainSettings.tMinHeight;
            maxHeight = terrainSettings.tMaxHeight;
        }
        else
        // otherwise
        {
            // set exact height value used
            exactHeightValue = terrainSettings.tExactHeight;
        }

        terrainShape = (int)terrainSettings.tShape;

        /*
        * Set river settings
        */
        riverGenerationEnabled = riverSettings.rGenerationEnabled;

        // if river generation is on
        if (riverGenerationEnabled)
        {
            // set the values used
            numRiver = (int)riverSettings.rNum;
            riverIntersectionsEnabled = riverSettings.intersectionsEnabled;
        }

        /*
        * Set river settings
        */
        lakeGenerationEnabled = lakeSettings.lGenerationEnabled;

        // if lake generation is on
        if (lakeGenerationEnabled)
        {
            // set the values used
            numLake = (int)lakeSettings.lNum;
            maxLakeSize = (int)lakeSettings.lMaxSize;
        }

        /*
        * Set river settings
        */
        walkpathGenerationEnabled = walkpathSettings.wGenerationEnabled;

        // if walkpath generation is on
        if (walkpathGenerationEnabled)
        {
            // set the values used
            numWalkpath = (int)walkpathSettings.wNum;
            walkpathIntersectionsEnabled = walkpathSettings.intersectionsEnabled;
        }
    }

    // check to see if any of the generation buttons have been pressed
    private void checkGenerationButtonPressed()
    {
        // if random generation or generate level have been pressed
        if (randomiseLevelPressed || generateLevelPressed)
        {
            // variables for the settings
            TerrainSettings terrainSettings;
            RiverSettings riverSettings;
            LakeSettings lakeSettings;
            WalkpathSettings walkpathSettings;

            // used to prevent level generation is height range is invalid (if on)
            bool generateLevel;

            // if random generation was pressed
            if (randomiseLevelPressed)
            {
                // set the settings to random values. no settings are given (other than
                // the terrain type from terrainSettings), so settings will be randomised.
                terrainSettings = new TerrainSettings();
                riverSettings = new RiverSettings(terrainSettings.tType);
                lakeSettings = new LakeSettings(terrainSettings.tType);
                walkpathSettings = new WalkpathSettings(terrainSettings.tType);

                // update the inspector with the settings used
                updateInspectorOptions(terrainSettings, riverSettings, lakeSettings, walkpathSettings);
                // set the generate level flag to true
                generateLevel = true;
            } else
            // otherwise generate level was pressed
            {
                // set the settings to the inspector options
                terrainSettings = new TerrainSettings((TerrainGenerator.TerrainType)terrainType, heightRangeEnabled, terrainSize, (TerrainGenerator.TerrainShape)terrainShape, minHeight, maxHeight, exactHeightValue);
                riverSettings = new RiverSettings((TerrainGenerator.TerrainType)terrainType, riverGenerationEnabled, (RiverGenerator.NumberOfRivers)numRiver, riverIntersectionsEnabled);
                lakeSettings = new LakeSettings((TerrainGenerator.TerrainType)terrainType, lakeGenerationEnabled, (LakeGenerator.NumberOfLakes)numLake, (LakeGenerator.MaxLakeSize)maxLakeSize);
                walkpathSettings = new WalkpathSettings((TerrainGenerator.TerrainType)terrainType, walkpathGenerationEnabled, (WalkpathGenerator.NumberOfWalkpaths)numWalkpath, walkpathIntersectionsEnabled);

                // if the height range toggle was on and the min and max values given were
                // invalid
                if (terrainSettings.heightRangeIsOnAndInvalid())
                {
                    // show popup with message
                    EditorUtility.DisplayDialog("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than or equal to the maximum value.", "Ok");
                    // set the generate level flag to false
                    generateLevel = false;
                }
                else
                // otherwise, continue to generation
                {
                    // update the terrain size option in the inspector with the value used
                    terrainSize = terrainSettings.tSize;
                    // set the generate level flag to true
                    generateLevel = true;
                }
            }

            // if generate level is on
            if (generateLevel)
            {
                // generate
                levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

                // update the generation info
                updateLevelGenInfo();
            }
        }
    }

    // adds a heading to the inspector
    private void addHeading(string headingText, bool mainHeading = false)
    {
        // add a space above the heading
        EditorGUILayout.Space();
        // change font size based on the mainHeading bool
        headingStyle.fontSize = mainHeading ? 15 : 13;
        // add the heading based on the headingText
        EditorGUILayout.LabelField(headingText, headingStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }
}
