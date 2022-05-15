using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing all operations to do with the user interface.
/// </summary>
public class UIManager : MonoBehaviour
{
    // '[SerializeField] private' show up in the inspector but are not accessible by other scripts

    // the level manager needed to pass data from the ui and start processes based on user input
    [SerializeField]
    private LevelManager levelManager;

    // the camera controller component for the level camera
    [SerializeField]
    private LevelCameraController levelCameraController;

    // the parent object of all level generation ui options
    [SerializeField]
    private GameObject levelGenUI;

    // the parent object of all demo ui options
    [SerializeField]
    private GameObject demoUI;

    // the script for handling popups
    [SerializeField]
    private PopupManager popupManager;

    // the terrain generation options
    [SerializeField]
    private TerrainOptions terrainOptions;

    // the river generation options
    [SerializeField]
    private RiverOptions riverOptions;

    // the lake generation options
    [SerializeField]
    private LakeOptions lakeOptions;

    // the walkpath generation options
    [SerializeField]
    private WalkpathPathOptions walkpathPathOptions;

    // the buttons on the user interface which interact
    // with elements which require a level to exist to be
    // used
    [SerializeField]
    private List<Button> levelInteractionButtons;

    // the level generation information to be displayed in the popup
    private string levelGenInfo;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {

        // do the user interface setup
        terrainOptions.setupUIElements();
        riverOptions.setupUIElements();
        lakeOptions.setupUIElements();
        walkpathPathOptions.setupUIElements();

        // set all the level interaction buttons to false
        foreach (Button button in levelInteractionButtons)
        {
            button.interactable = false;
        }
    }


    /// <summary>
    /// Calls the level camera controller to be recentered around the level.
    /// Called when the recenter button is pressed.
    /// </summary>
    public void recenterCamera()
    {
            // recenter the camera around the level
            levelCameraController.recenterCamera();
    }

    /// <summary>
    /// Calls the level camera controller to zoom into the level. Called when
    /// the zoom in button is pressed.
    /// </summary>
    public void zoomIn()
    {
            // zoom into the level
            levelCameraController.zoomIn();
    }

    /// <summary>
    /// Calls the level camera controller to zoom out of the level. Called
    /// when the zoom out button is pressed.
    /// </summary>
    public void zoomOut()
    {
            // zoom out of the level
            levelCameraController.zoomOut();
    }

    /// <summary>
    /// Creates and validates the settings before calling the level manager to generate the level.
    /// Called when the generate level button is pressed.
    /// </summary>
    public void generateLevel()
    {
        // create the terrain settings from the users options
        TerrainSettings terrainSettings =  terrainOptions.createUserSettingsFromOptions();

        // check if the height range option is on and is invalid (minimum height greater
        // than or equal to the maximum height
        if (terrainSettings.heightRangeIsOnAndInvalid())
        {
            // invalid settings, show popup
            popupManager.showPopup("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than or equal to the maximum value.");
            // end method execution
            return;
        }

        // create the other settings from the users options
        RiverSettings riverSettings = riverOptions.createUserSettingsFromOptions();
        LakeSettings lakeSettings = lakeOptions.createUserSettingsFromOptions();
        WalkpathSettings walkpathSettings = walkpathPathOptions.createUserSettingsFromOptions();

        // generate the level
        levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

        // update the terrain size in the ui with the setting used.
        terrainOptions.updateTerrainSizeField(terrainSettings.tSize);

        updateButtonsandInfo(terrainSettings.heightRangeEnabled);
    }

    /// <summary>
    /// Creates randomised settings before calling the level manager to generate the level with 
    /// the randomised settings. Called when the randomise level button is pressed.
    /// </summary>
    public void generateRandomLevel()
    {
        // create the randomised settings
        TerrainSettings terrainSettings = new TerrainSettings();
        RiverSettings riverSettings = new RiverSettings(terrainSettings.tType);
        LakeSettings lakeSettings = new LakeSettings(terrainSettings.tType);
        WalkpathSettings walkpathSettings = new WalkpathSettings(terrainSettings.tType);

        // generate the level with the settings
        levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

        // update the user interface fields to reflect the settings used
        terrainOptions.updateFields(terrainSettings);
        riverOptions.updateFields(riverSettings);
        lakeOptions.updateFields(lakeSettings);
        walkpathPathOptions.updateFields(walkpathSettings);

        // update the ui
        updateButtonsandInfo(terrainSettings.heightRangeEnabled);
    }

    // updates the ui buttons and sets the level generation information
    private void updateButtonsandInfo(bool heightRangeEnabled)
    {
        // enable the level interaction buttons
        foreach (Button button in levelInteractionButtons)
        {
            button.interactable = true;
        }

        // disable the demo level button for range height levels
        // (collision on range height levels needs work)      
        levelInteractionButtons.First().interactable = !heightRangeEnabled;

        // set the level generation information
        setLevelGenInfo();
    }

    // set the level generation information
    private void setLevelGenInfo()
    {
        // concatenate the generation steps with newline as seperator
        levelGenInfo = string.Join("\n", levelManager.getGenerationInfo());
        levelGenInfo += "\n";
    }

    /// <summary>
    /// Show the level generation information. Called when the level info
    /// button is pressed.
    /// </summary>
    public void showLevelInfo()
    {
        popupManager.showPopup("Level Generation Information", levelGenInfo);
    }

    /// <summary>
    /// Switch to the demo mode for the level generated. Called when the demo
    /// level button is pressed.
    /// </summary>
    public void demoLevel()
    {
            // disable the level generation ui
            levelGenUI.SetActive(false);
            // enable the demo mode ui
            demoUI.SetActive(true);

            // setup the player. temp initial position. position setting will be more complicated
            levelManager.setupPlayer();
            // disable the level camera
            levelManager.setLevelCameraActive(false); 
    }

    /// <summary>
    /// Returns the user back to the level generation user interface from the demo user interface.
    /// Called when the exit level button is pressed.
    /// </summary>
    public void exitLevel()
    {
        // enable the level camera
        levelManager.setLevelCameraActive(true);
        // disable the player
        levelManager.setPlayerActive(false);

        // disable the demo mode ui
        demoUI.SetActive(false);
        // enable the level generation ui
        levelGenUI.SetActive(true);
    }
    
    /// <summary>
    /// Closes an opened popup. Used by the close button on the popup panel.
    /// </summary>
    public void closePopup()
    {
        // tell the popup manager to hide the popup.
        popupManager.hidePopup();
    }
}
