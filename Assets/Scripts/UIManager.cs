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
    [SerializeField]
    private GameObject levelGenUI;

    // the parent object of all demo ui options
    [SerializeField]
    private GameObject demoUI;

    // the script for handling popups
    [SerializeField]
    private PopupManager popupManager;

    // the slider for the terrain size
    [SerializeField]
    private Slider terrainSizeSlider;

    // the input field for the terrain size
    [SerializeField]
    private InputField terrainSizeInput;

    // the dropdown for the terrain type
    [SerializeField]
    private Dropdown terrainTypeDropdown;

    [SerializeField]
    private Toggle terrainExactHeightToggle;

    [SerializeField]
    private Toggle terrainRangeHeightToggle;

    [SerializeField]
    private GameObject terrainExactHeightOptions;

    [SerializeField]
    private GameObject terrainRangeHeightOptions;

    [SerializeField]
    private Slider terrainExactHeightSlider;

    [SerializeField]
    private InputField terrainExactHeightInput;

    [SerializeField]
    private Slider terrainRangeHeightMinSlider;

    [SerializeField]
    private InputField terrainRangeHeightMinInput;

    [SerializeField]
    private Slider terrainRangeHeightMaxSlider;

    [SerializeField]
    private InputField terrainRangeHeightMaxInput;

    [SerializeField]
    private Dropdown terrainShapeDropdown;



    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // get the layer value for ui elements
        UILayer = LayerMask.NameToLayer("UI");

        // do the user interface setup after retieving the elements
        setupUI();
    }

    //  setup the user interface with values and bounds
    private void setupUI()
    {
        /*
         * terrain size setup
         */

        // setup the terrain size slider
        setupSlider(terrainSizeSlider, terrainSizeInput, TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);

        /*
        * terrain type setup
        */

        // setup the terrain type dropdown
        setupDropdown(terrainTypeDropdown, Enum.GetNames(typeof(TerrainGenerator.terrainType)).ToList());

        /*
        * terrain height setup
        */

        // if the first toggle to be active is the terrain exact height toggle

        setupToggle(terrainExactHeightToggle, terrainExactHeightOptions);
        setupToggle(terrainRangeHeightToggle, terrainRangeHeightOptions);

        // setup the terrain exact height slider
        setupSlider(terrainExactHeightSlider, terrainExactHeightInput, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);

        // setup the terrain range height min slider
        setupSlider(terrainRangeHeightMinSlider, terrainRangeHeightMinInput, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        // setup the terrain range height min slider
        setupSlider(terrainRangeHeightMaxSlider, terrainRangeHeightMaxInput, TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);

        /*
         * terrain shape setup
         */

        // setup the terrain shape dropdown
        setupDropdown(terrainShapeDropdown, Enum.GetNames(typeof(TerrainGenerator.terrainShape)).ToList());

    }

    private void setupToggle(Toggle toggle, GameObject option)
    {
        // make sure the options panel is initially in the correct state relative to the toggle state
        toggleOption(toggle, option);
        // add the toggle option to the on value change listener
        toggle.onValueChanged.AddListener( delegate { toggleOption(toggle, option); });
    }

    // common setup tasks to be done for dropdowns
    private void setupDropdown(Dropdown dropdown, List<string> options)
    {
        // clear the dropdown
        dropdown.ClearOptions();
        // add the options
        dropdown.AddOptions(options);
    }

    // common setup tasks to be done for sliders
    private void setupSlider(Slider slider, InputField input, int minValue, int maxValue)
    {
        // limit slider to be between the min and max values
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        // set the input field to be equal to the sliders value
        input.text = slider.value.ToString("0");

        slider.onValueChanged.AddListener(delegate { updateSliderField(slider, input); });
        input.onEndEdit.AddListener(delegate { checkSliderField(input, minValue, maxValue); });
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
    /// Called anytime a slider changes to update the input field with the slider value.
    /// </summary>
    /// <param name="slider"></param>
    /// <param name="input"></param>
    public void updateSliderField(Slider slider, InputField input)
    {
        input.text = slider.value.ToString("0");
    }

    /// <summary>
    /// Called anytime an input field has been edited to ensure the input is valid
    /// </summary>
    public void checkSliderField(InputField input, int minValue, int maxValue)
    {
        if (input.text.Length != 0)
        {
            if (int.Parse(input.text) < minValue)
            {
                terrainSizeInput.text = minValue.ToString("0");
            }
            else if (int.Parse(input.text) > maxValue)
            {
                input.text = maxValue.ToString("0");
            }
        }
    }

    public void toggleOption(Toggle toggle, GameObject option)
    {
        option.SetActive(toggle.isOn ? true : false);
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
