using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the level camera's functionality that is used in the user interface.
/// </summary>
public class LevelCameraController : MonoBehaviour
{
    // '[SerializeField] private' show up in the inspector but are not accessible by other scripts

    // the speed of the camera zooming in
    [SerializeField]
    private float zoomSpeed;
    
    // the camera for the level
    private Camera levelCamera;

    // holds the center position of the level
    private Vector3 levelCenter;

    // the value that positions the level camera such that the level is besides the settings panel
    // in the user interface
    private Vector3 cameraOffset;

    // the camera's orthographic size after the level is initially generated.
    // it is set to fit the entire level on the screen
    private float defaultOrthoSize;

    // whether or not click and drag is enabled
    private bool dragEnabled;

    // where the mouse initiated the drag
    private Vector3 mouseOrigin;

    // difference between mouse current position and the cameras position
    private Vector3 difference;

    // whether or not a level is generated
    private bool levelIsGenerated;

    // layer value for ui elements
    private int UILayer;

    // the original point at which the mouse button was pressed
    private Vector3 originalMouseDownPosition;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // set the ref to the camera component
        levelCamera = GetComponent<Camera>();
        // ensure the levelIsGenerated bool is fault at the start
        levelIsGenerated = false;
        // get the layer value for ui elements
        UILayer = LayerMask.NameToLayer("UI");
    }

    // late update is called every frame when the script is enabled, after update
    private void LateUpdate()
    {
        // if the mousebutton is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // set the original mouse down position to be the world position of where the mouse was pressed
            originalMouseDownPosition = levelCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        // if the level is generated and the mouse is not over any ui element
        if (levelIsGenerated && !IsPointerOverUIElement())
        {
            // use the click and drag functionality
            clickAndDrag();
            // use the scroll to zoom functionality
            scrollZoom();
        }
    }

    /// <summary>
    /// Updates the values important for the camera to be position correctly relative to the
    /// level generated. Called after a level is generated.
    /// </summary>
    /// <param name="newlevelCenter">The center of the new level generated.</param>
    /// <param name="newOrthoSize">The new size of the orthographic camera window</param>
    public void updateCamera(Vector3 newlevelCenter, float newOrthoSize)
    {
        // set the level center to be the new level center
        levelCenter = newlevelCenter;
        // set the camera offset to be half of the new orthographic size in the positive x direction
        cameraOffset = new Vector3(newOrthoSize * 0.5f, 0, 0);
        // set the default orthographic size to be the new orthographic size
        defaultOrthoSize = newOrthoSize;
        // the level is now generated, so set the levelIsGenerated bool to true
        levelIsGenerated = true;

        // center the camera around the new level
        recenterCamera();
    }

    /// <summary>
    /// Recenter the camera around the level generated.
    /// </summary>
    public void recenterCamera()
    {
            // set the level cameras position to at the center of level plus the offset
            levelCamera.transform.position = levelCenter + cameraOffset;
            // set the level cameras orthographic size to be the default size
            levelCamera.orthographicSize = defaultOrthoSize;
    }

    /// <summary>
    /// Zoom into the level generated
    /// </summary>
    public void zoomIn()
    {
        // if the level camera's orthographic size is greater than 1
        if (levelCamera.orthographicSize > 1)
        {
            // reduce the orthographic size by the amount defined in zoomSpeed
            levelCamera.orthographicSize -= zoomSpeed;
        }
    }

    /// <summary>
    /// Zoom out of the level generated
    /// </summary>
    public void zoomOut()
    {
        // if the level camera's orthographic size is less than the default orthographic size
        if (levelCamera.orthographicSize < defaultOrthoSize)
        {
            // increase the orthographic size by the amount defined in zoomSpeed
            levelCamera.orthographicSize += zoomSpeed;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void clickAndDrag()
    {
        if (Input.GetMouseButton(0))
        {
            difference = levelCamera.ScreenToWorldPoint(Input.mousePosition) - levelCamera.transform.position;

            // comparing original position to current to avoid starting a mouse down on a ui element
            if (!dragEnabled && originalMouseDownPosition == levelCamera.ScreenToWorldPoint(Input.mousePosition))
            {
                dragEnabled = true;
                mouseOrigin = levelCamera.ScreenToWorldPoint(Input.mousePosition);
            }

        }
        else
        {
            dragEnabled = false;
        }

        if (dragEnabled)
        {
            levelCamera.transform.position = mouseOrigin - difference;
        }
    }

    private void scrollZoom()
    {
        float mouseScrollDeltaY = Input.mouseScrollDelta.y;

        // limit zooming to be between 1 and the default size of the orthographic window
        if ((levelCamera.orthographicSize > 1 && mouseScrollDeltaY > 0) || (levelCamera.orthographicSize < defaultOrthoSize && mouseScrollDeltaY < 0))
        {
            levelCamera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
        }
    }

    /*
     * code below from https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
     */

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
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
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
