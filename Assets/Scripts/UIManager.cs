using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// this class can be heavily refactored to group serialised fields and loop setup
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

    [SerializeField]
    private TerrainOptions terrainOptions;

    [SerializeField]
    private RiverOptions riverOptions;

    [SerializeField]
    private LakeOptions lakeOptions;

    [SerializeField]
    private WalkpathPathOptions walkpathPathOptions;

    [SerializeField]
    private List<Button> levelInteractionButtons;

    string levelGenInfo;
    // start is called before the first frame update when the script is enabled
    private void Start()
    {

        // do the user interface setup
        terrainOptions.setupUIElements();
        riverOptions.setupUIElements();
        lakeOptions.setupUIElements();
        walkpathPathOptions.setupUIElements();

        foreach (Button button in levelInteractionButtons)
        {
            button.interactable = false;
        }
    }


    /// <summary>
    /// Tell the level camera to recenter around the level.
    /// </summary>
    public void recenterCamera()
    {
            // recenter the camera around the level
            levelCameraController.recenterCamera();
    }

    /// <summary>
    /// Tell the level camera to zoom into the level.
    /// </summary>
    public void zoomIn()
    {
            // zoom into the level
            levelCameraController.zoomIn();
    }

    /// <summary>
    /// Tell the level camera to zoom out of the level.
    /// </summary>
    public void zoomOut()
    {
            // zoom out of the level
            levelCameraController.zoomOut();
    }

    /// <summary>
    /// Tell the level manager to generate the level.
    /// </summary>
    public void generateLevel()
    {
        levelGenInfo = null;

        TerrainOptions.TerrainSettings terrainSettings =  terrainOptions.createUserSettings();

        if (terrainSettings.heightRangeIsOnAndInvalid())
        {
            popupManager.showPopup("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than or equal to the maximum value.");
            return;
        }

        RiverOptions.RiverSettings riverSettings = riverOptions.createUserSettings();
        LakeOptions.LakeSettings lakeSettings = lakeOptions.createUserSettings();
        WalkpathPathOptions.WalkpathSettings walkpathSettings = walkpathPathOptions.createUserSettings();

        // generate the level
        levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

        terrainOptions.updateTerrainSizeField(terrainSettings.tSize);

        foreach (Button button in levelInteractionButtons)
        {
            button.interactable = true;
        }

        if (terrainSettings.heightRangedEnabled)
        {
            levelInteractionButtons.First().interactable = false;
        }

        foreach (string generationStep in levelManager.getGenerationInfo())
        {
            levelGenInfo += generationStep + "\n";
        }
    }

    /// <summary>
    /// Tell the level manager to generate the level with randomised settings.
    /// </summary>
    public void generateRandomLevel()
    {
        levelGenInfo = null;

        TerrainOptions.TerrainSettings terrainSettings = terrainOptions.createRandomisedSettings();
        RiverOptions.RiverSettings riverSettings = riverOptions.createRandomisedSettings();
        LakeOptions.LakeSettings lakeSettings = lakeOptions.createRandomisedSettings();
        WalkpathPathOptions.WalkpathSettings walkpathSettings = walkpathPathOptions.createRandomisedSettings();

        // generate the level
        levelManager.generate(terrainSettings, riverSettings, lakeSettings, walkpathSettings);

        terrainOptions.updateFields(terrainSettings);
        riverOptions.updateFields(riverSettings);
        lakeOptions.updateFields(lakeSettings);
        walkpathPathOptions.updateFields(walkpathSettings);

        foreach (Button button in levelInteractionButtons)
        {
            button.interactable = true;
        }

        if (terrainSettings.heightRangedEnabled)
        {
            levelInteractionButtons.First().interactable = false;
        }

        foreach (string generationStep in levelManager.getGenerationInfo())
        {
            levelGenInfo += generationStep + "\n";
        }
    }

    // make panel expand to scrollable if text is too long
    public void showLevelInfo()
    {
        popupManager.showPopup("Level Generation Information", levelGenInfo);
    }

    /// <summary>
    /// Switch to the demo mode for the level generated.
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
    /// Closes an opened popup. Used by the close button on the popup panel
    /// </summary>
    public void closePopup()
    {
        // tell the popup manager to hide the popup.
        popupManager.hidePopup();
    }
}
