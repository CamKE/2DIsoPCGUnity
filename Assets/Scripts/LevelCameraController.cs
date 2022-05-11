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

    // the original point at which the left mouse button was pressed
    private Vector3 originalMouseDownPosition;

    // layer value for ui elements
    private int uiLayer;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // get the layer value for ui elements
        uiLayer = LayerMask.NameToLayer("UI");
        // set the ref to the camera component
        levelCamera = GetComponent<Camera>();
    }


    // late update is called every frame when the script is enabled, after update
    private void LateUpdate()
    {
        // if the left mousebutton is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // set the original mouse down position to be the world position of where the left mouse button was pressed
            setOriginalMouseDownPosition();
        }

        // if the level is generated and the mouse is not over any ui element
        if (!isPointerOverUIElement())
        {
            // use the click and drag functionality
            clickAndDrag();
            // use the scroll to zoom functionality
            scrollZoom();
        }
    }

    /// <summary>
    /// Setter for the <see cref="originalMouseDownPosition"/>, used by the <see cref="UIManager"/>
    /// </summary>
    public void setOriginalMouseDownPosition()
    {
        // set the original mouse down position to be the world position of where the left mouse button was pressed
        originalMouseDownPosition = levelCamera.ScreenToWorldPoint(Input.mousePosition);
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
    /// Functionality that uses the mouse as input to click and drag the level camera into position.
    /// </summary>
    public void clickAndDrag()
    {
        // if the left mouse button is held down
        if (Input.GetMouseButton(0))
        {
            // calculate the difference between the position of the mouse in the world and the position of the level
            // camera in the world
            difference = levelCamera.ScreenToWorldPoint(Input.mousePosition) - levelCamera.transform.position;

            // if drag is not enabled
            // comparing original position to current to avoid starting a mouse down on a ui element
            if (!dragEnabled && originalMouseDownPosition == levelCamera.ScreenToWorldPoint(Input.mousePosition))
            {
                // enable drag
                dragEnabled = true;
                // set the origin of the mouse down to be the current position of the mouse 
                mouseOrigin = levelCamera.ScreenToWorldPoint(Input.mousePosition);
            }

        }
        // otherwise
        else
        {
            // disable drag
            dragEnabled = false;
        }

        // if drag is enabled
        if (dragEnabled)
        {
            // move the level camera by the difference, relative to the origin of the mouse down
            // (where the drag was started)
            levelCamera.transform.position = mouseOrigin - difference;
        }
    }

    /// <summary>
    /// functionality to zoom the level camera using a mouse scrollwheel. zoom out, and ortho size increases,
    /// zoom in and ortho size decreases
    /// </summary>
    public void scrollZoom()
    {
        // get the direction of the mouse vertical scroll. 0 if no scroll, 1.0 for forward, -1.0 for backward
        float mouseScrollDeltaY = Input.mouseScrollDelta.y;

        // limit zooming to be between 1 and the default size of the orthographic window
        // if the orthographic size is greater than 1 AND the scroll y delta is forward
        // OR 
        // the orthographic size is less than the default size for the level AND the scroll y delta is backward
        if ((levelCamera.orthographicSize > 1 && mouseScrollDeltaY > 0) || (levelCamera.orthographicSize < defaultOrthoSize && mouseScrollDeltaY < 0))
        {
            // reduce the orthographic size by the zoom speed in the direction denoted by the vertical(y) mouse scroll delta
            levelCamera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
        }
    }

    /*
    * code below from https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    */

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool isPointerOverUIElement()
    {
        return isPointerOverUIElement(getEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool isPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        for (int index = 0; index < eventSystemRaycastResults.Count; index++)
        {
            RaycastResult curRaycastResult = eventSystemRaycastResults[index];
            if (curRaycastResult.gameObject.layer == uiLayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> getEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
