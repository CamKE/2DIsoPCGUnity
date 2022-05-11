using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enables game objects to have text displayed providing useful information when hovered over.
/// </summary>
/// <remarks>
/// <para>
/// This custom component is given to user interface game object, and is given an associated text
/// to be displayed when the object is hovered over.
/// </para>
/// <para>
/// The <see cref="IPointerEnterHandler">IPointerEnterHandler</see> and <see cref="IPointerExitHandler">IPointerExitHandler</see>
/// are used to detect when the mouse enter and exits the gameobject associated with the <c>HoverTip</c> instance.
/// </para>
/// </remarks>
public class HoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// The tip text to be displayed in the tip window for the associated object
    /// </summary>
    public string tipToShow;
    // how long to wait before displaying the tip (after mouse hover registered)
    private const float timeToWait = 0.5f;

    /// <summary>
    /// Called when the associated gameobject is entered.
    /// </summary>
    /// <param name="eventData">Provides detailed information about the pointer enter event.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any currently running coroutines
        StopAllCoroutines();
        // Start the startTimer coroutine
        StartCoroutine(startTimer());
    }

    /// <summary>
    /// Called when the associated gameobject is exited.
    /// </summary>
    /// <param name="eventData">Provides detailed information about the pointer exit event.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        HoverTipManager.onMouseLoseFocus();
    }

    // call the manager to show the message (tip) for the object associated with this hovertip instance
    private void showMessage()
    {
        // call the delegate function and give it the tip to be displayed, and the position
        // of the mouse
        HoverTipManager.onMouseHover(tipToShow, Input.mousePosition);
    }

    // a coroutine to wait before displaying the message (tip)
    private IEnumerator startTimer()
    {
        // wait until the time has passed before...
        yield return new WaitForSeconds(timeToWait);

        // displaying the message (tip)
        showMessage();
    }
}
