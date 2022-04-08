using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// this class can be heavily refactored such that events for the elements return themselves to be
// modified. this also means i wont need a seperate method for each e.g. slider to update the input field.
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
    private int UILayer;
    
    // the parent object of all level generation ui options
    private GameObject levelGenUI;

    // the parent object of all demo ui options
    private GameObject demoUI;

    // the script for handling popups
    private PopupManager popupManager;

    // the slider for the terrain size
    private Slider terrainSizeSlider;

    // the input field for the terrain size
    private InputField terrainSizeInput;

    // the dropdown for the terrain shape
    private Dropdown terrainShapeDropdown;

    private ToggleGroup terrainHeightTG;

    private GameObject terrainExactHeightOptions;

    private GameObject terrainRangeHeightOptions;

    private Slider terrainExactHeightSlider;

    private InputField terrainExactHeightInput;


    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // get the layer value for ui elements
        UILayer = LayerMask.NameToLayer("UI");

        // get the levelgenui gameobject to enable and disable
        levelGenUI = this.gameObject.transform.GetChild(0).gameObject;

        // get the demoui gameobject to enable and disable
        demoUI = this.gameObject.transform.GetChild(1).gameObject;

        // get the popupmanager to generate popups
        popupManager = this.gameObject.transform.GetChild(2).gameObject.GetComponent<PopupManager>();

        // the transform content panel
        Transform contentPanel = levelGenUI.gameObject.transform.GetChild(0).GetChild(0).GetChild(0);

        // the transform of the terrain section
        Transform terrainSection = contentPanel.transform.GetChild(1);

        // the transform of the water bodies section
        Transform waterBodiesSection = contentPanel.transform.GetChild(2);

        // get the terrain size inputs relative to the terrain section transform
        terrainSizeSlider = terrainSection.GetChild(0).GetChild(1).GetComponent<Slider>();
        terrainSizeInput = terrainSection.GetChild(0).GetChild(2).GetComponent<InputField>();

        // get the terrain shape dropdown relative to the terrain section transform
        terrainShapeDropdown = terrainSection.GetChild(1).GetChild(1).GetComponent<Dropdown>();

        // get the terrain height toggle group relative to the terrain section transform
        terrainHeightTG = terrainSection.GetChild(2).GetComponent<ToggleGroup>();
        // get the terrain exact height options gameobject relative to the terrain section transform
        terrainExactHeightOptions = terrainSection.GetChild(3).gameObject;
        // get the terrain range height options gameobject relative to the terrain section transform
        terrainRangeHeightOptions = terrainSection.GetChild(4).gameObject;

        // get the terrain exact height slider relative to the terrainExactHeightOptions section
        terrainExactHeightSlider = terrainExactHeightOptions.transform.GetChild(1).GetComponent<Slider>();
        // get the terrain exact height input field relative to the terrainExactHeightOptions section
        terrainExactHeightInput = terrainExactHeightOptions.transform.GetChild(2).GetComponent<InputField>();

        // do the user interface setup after retieving the elements
        setupUI();
    }

    //  setup the user interface with values and bounds
    private void setupUI()
    {
        /*
         * terrain size setup
         */

        // limit terrain size to be between the min and max size
        terrainSizeSlider.minValue = TerrainGenerator.terrainMinSize;
        terrainSizeSlider.maxValue = TerrainGenerator.terrainMaxSize;
        // set the terrain size input field to be equal to the sliders value
        terrainSizeInput.text = terrainSizeSlider.value.ToString("0");

         /*
         * terrain type/shape setup
         */

        // clear the dropdown
        terrainShapeDropdown.ClearOptions();
        // add the terrain shape options
        terrainShapeDropdown.AddOptions(Enum.GetNames(typeof(TerrainGenerator.terrainShape)).ToList());

         /*
         * terrain height setup
         */

        // if the first toggle to be active is the terrain exact height toggle
        if (terrainHeightTG.GetFirstActiveToggle().name == terrainExactHeightOptions.name)
        {
            // show the exact height options only
            terrainExactHeightOptions.SetActive(true);
            terrainRangeHeightOptions.SetActive(false);
        } else
        // otherwise
        {
            // show the range height options only
            terrainExactHeightOptions.SetActive(false);
            terrainRangeHeightOptions.SetActive(true);
        }

        // limit terrain exact height to be between the min and max values
        terrainExactHeightSlider.minValue = TerrainGenerator.terrainMinHeight;
        terrainExactHeightSlider.maxValue = TerrainGenerator.terrainMaxHeight;
        // set the terrain exact height input field to be equal to the sliders value
        terrainExactHeightInput.text = terrainExactHeightSlider.value.ToString("0");


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
        // generate the level
        levelManager.generate();
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
            levelManager.setupPlayer(new Vector3(1, 1, 3));
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

    /// <summary>
    /// Called anytime the terrain size slider is changed. Used to update
    /// the corresponding input field.
    /// </summary>
    public void updateTerrainSizeField()
    {
        // set the terrain size input field to be the sliders current value
        terrainSizeInput.text = terrainSizeSlider.value.ToString("0");
    }

    /// <summary>
    /// Called anytime the terrain exact height slider is changed. Used to update
    /// the corresponding input field.
    /// </summary>
    public void updateTerrainExactHeightField()
    {
        // set the terrain size input field to be the sliders current value
        terrainExactHeightInput.text = terrainExactHeightSlider.value.ToString("0");
    }

    public void checkTerrainInputSizeValue()
    {
        if (terrainSizeInput.text.Length != 0)
        {
            if (int.Parse(terrainSizeInput.text) < TerrainGenerator.terrainMinSize)
            {
                terrainSizeInput.text = TerrainGenerator.terrainMinSize.ToString("0");
            }
            else if (int.Parse(terrainSizeInput.text) > TerrainGenerator.terrainMaxSize)
            {
                terrainSizeInput.text = TerrainGenerator.terrainMaxSize.ToString("0");
            }
        }
    }

    public void toggleExactHeightOption()
    {
        terrainRangeHeightOptions.SetActive(false);
        terrainExactHeightOptions.SetActive(true);
    }

    public void toggleHeightRangeOption()
    {
        terrainExactHeightOptions.SetActive(false);
        terrainRangeHeightOptions.SetActive(true);
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
            if (curRaysastResult.gameObject.layer == UILayer)
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
