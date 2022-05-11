using UnityEngine;
using System;
using TMPro;

// Credit to Comp-3 Interative for the tutorial
/// <summary>
/// This class is responsible for managing the functionality of the
/// <see cref="HoverTip"/>.
/// </summary>
public class HoverTipManager : MonoBehaviour
{
    
    /// <summary>
    ///  The text to be displayed in the tip window.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI tipText;

    /// <summary>
    /// The 'floating' window which holds the tip text.
    /// </summary>
    [SerializeField]
    private RectTransform tipWindow;

    /// <summary>
    /// Delegate for response to the mouse hover over a element.
    /// </summary>
    public static Action<string, Vector2> onMouseHover;

    /// <summary>
    /// Delegate for response to the mouse no longer hovering over a element.
    /// </summary>
    public static Action onMouseLoseFocus;

    private const float tipWindowXOffset = 50.0f;

    private const float fullHDWidth = 1920.0f;

    // start is called before the first frame update when the script is enabled
    private void Start()
    {
        // hide the tip upon application start
        hideTip();
        // top left pivot
        tipWindow.pivot = Vector2.up;
    }

    // called when an instance of this class is created.
    private void OnEnable()
    {
        // assign the function showTip to the onMouseHover delegate
        onMouseHover += showTip;
        // assign the function hideTip to the onMouseLoseFocus delegate
        onMouseLoseFocus += hideTip;
    }

    // called when an instance of this class is destroyed
    private void OnDisable()
    {
        // remove the function showTip from the onMouseHover delegate
        onMouseHover -= showTip;
        // remove the function hideTip to the onMouseLoseFocus delegate
        onMouseLoseFocus -= hideTip;
    }

    // Responsible for displaying a given tip inside the tip window relative to the given postion
    private void showTip(string tip, Vector2 mousePos)
    {
        // assign the new text to be displayed to the current tip text 
        tipText.text = tip;

        // define the size of the tip window
        // tip window width should be at most 200
        // tip window height is set to text height
        tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 200 ? 200 : tipText.preferredWidth, tipText.preferredHeight);
        // make the tip window object active
        tipWindow.gameObject.SetActive(true);
        // place the tip window slighty to the right
        // of the position where the tip was activated
        
        tipWindow.transform.position = new Vector2(mousePos.x + (tipWindowXOffset * (Screen.width / fullHDWidth)), mousePos.y);
    }

    // Responsible for hiding the tip window
    private void hideTip()
    {
        // remove the previous tip from the tiptext (set to null)
        tipText.text = default;
        // deactivate the tip window object
        tipWindow.gameObject.SetActive(false);
    }
}
