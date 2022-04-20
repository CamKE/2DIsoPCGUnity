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

    // layer value for ui elements
    private int uiLayer;

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

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // get the layer value for ui elements
        uiLayer = LayerMask.NameToLayer("UI");

        // do the user interface setup
        terrainOptions.setupUIElements();
        riverOptions.setupUIElements();
        lakeOptions.setupUIElements();
    }

    // late update is called every frame when the script is enabled, after update
    private void LateUpdate()
    {
        // if the left mousebutton is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // set the original mouse down position to be the world position of where the left mouse button was pressed
            levelCameraController.setOriginalMouseDownPosition();
        }

        // if the level is generated and the mouse is not over any ui element
        if (levelManager.levelisGenerated && !IsPointerOverUIElement())
        {
            // use the click and drag functionality
            levelCameraController.clickAndDrag();
            // use the scroll to zoom functionality
            levelCameraController.scrollZoom();
        }
    }

    /// <summary>
    /// Tell the level camera to recenter around the level.
    /// </summary>
    public void recenterCamera()
    {
        // if the level is generated 
        if (levelManager.levelisGenerated)
        {
            // recenter the camera around the level
            levelCameraController.recenterCamera();
        }
    }

    /// <summary>
    /// Tell the level camera to zoom into the level.
    /// </summary>
    public void zoomIn()
    {
        // if the level is generated 
        if (levelManager.levelisGenerated)
        {
            // zoom into the level
            levelCameraController.zoomIn();
        }
    }

    /// <summary>
    /// Tell the level camera to zoom out of the level.
    /// </summary>
    public void zoomOut()
    {
        // if the level is generated 
        if (levelManager.levelisGenerated)
        {
            // zoom out of the level
            levelCameraController.zoomOut();
        }
    }

    /// <summary>
    /// Tell the level manager to generate the level.
    /// </summary>
    public void generateLevel()
    {
        if (terrainOptions.heightRangeIsOnAndInvalid())
        {
            popupManager.showPopup("Invalid Terrain Height Range", "Terrain height minimum value cannot be greater than the maximum value.");
            return;
        }

        TerrainOptions.TerrainSettings terrainSettings =  terrainOptions.createUserSettings();
        RiverOptions.RiverSettings riverSettings = riverOptions.createUserSettings();
        LakeOptions.LakeSettings lakeSettings = lakeOptions.createUserSettings();

        // generate the level
        levelManager.generate(terrainSettings, riverSettings, lakeSettings);
    }

    /// <summary>
    /// Tell the level manager to generate the level with randomised settings.
    /// </summary>
    public void randomlyGenerateLevel()
    {

    }

    /// <summary>
    /// Switch to the demo mode for the level generated.
    /// </summary>
    public void demoLevel()
    {
        // if the level is generated
        if (levelManager.levelisGenerated)
        {
            // disable the level generation ui
            levelGenUI.SetActive(false);
            // enable the demo mode ui
            demoUI.SetActive(true);

            // setup the player. temp initial position. position setting will be more complicated
            levelManager.setupPlayer();
            // disable the level camera
            levelManager.setLevelCameraActive(false);
        } else
        // otherwise
        {
            // Show a popup message to the user
            popupManager.showPopup("No Level Exists", "There is no level to be demoed.");
        }
     

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

    /*
    * code below from https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    */

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == uiLayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
