using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelCameraController : MonoBehaviour
{
    [SerializeField]
    private float scrollZoomSpeed;

    private Camera levelCamera;

    // holds the center position of the level
    private Vector3 levelCenter;

    private Vector3 cameraOffset;

    private float defaultOrthoSize;

    // whether or not click and drag is enabled
    private bool dragEnabled;

    // where the mouse initiated the drag
    private Vector3 mouseOrigin;

    // difference between mouse current position and the cameras position
    private Vector3 difference;

    private bool levelIsGenerated;

    private int UILayer;

    private Vector3 originalMouseDownPosition;

    // Start is called before the first frame update
    void Start()
    {
        levelCamera = GetComponent<Camera>();
        levelIsGenerated = false;
        UILayer = LayerMask.NameToLayer("UI");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            originalMouseDownPosition = levelCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (levelIsGenerated && !IsPointerOverUIElement())
        {
            clickAndDrag();
            scrollZoom();
        }
    }

    public void updateCamera(Vector3 levelCenter, Vector3 cameraOffset, float newOrthoSize)
    {
        this.levelCenter = levelCenter;
        this.cameraOffset = cameraOffset;
        this.defaultOrthoSize = newOrthoSize;
        levelIsGenerated = true;

        recenterCamera();
    }

    public void recenterCamera()
    {
        levelCamera.transform.position = levelCenter + cameraOffset;
        levelCamera.orthographicSize = defaultOrthoSize;
    }

    public void zoomIn()
    {
        if (levelCamera.orthographicSize > 1)
        {
            levelCamera.orthographicSize -= scrollZoomSpeed;
        }
    }

    public void zoomOut()
    {
        if (levelCamera.orthographicSize < defaultOrthoSize)
        {
            levelCamera.orthographicSize += scrollZoomSpeed;
        }
    }

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
            levelCamera.orthographicSize -= Input.mouseScrollDelta.y * scrollZoomSpeed;
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
