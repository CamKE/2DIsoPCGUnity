using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Called by the user interface to show and hide popups with custom heading and body text.
/// </summary>
public class PopupManager : MonoBehaviour
{
    // the heading text for the popup
    [SerializeField]
    private Text popupHeading;

    // the body text for the popup
    [SerializeField]
    private Text popupBody;

    // the content rect in the scroll view
    [SerializeField]
    private RectTransform popupContent;

    /// <summary>
    /// Activates the popup with the given heading and body text.
    /// </summary>
    /// <param name="messageHeading">The heading of the popup to be shown.</param>
    /// <param name="messageBody">The body message of the popup to be shown.</param>
    public void showPopup(string messageHeading, string messageBody)
    {
            // set the popup heading to be the given heading
            popupHeading.text = messageHeading;
            // set the popup body to be the given body
            popupBody.text = messageBody;
            // set the content rect height to be the ideal height specified from the body text
            // vertical scroll will be updated based on the content rect height
            popupContent.sizeDelta = new Vector2(0, popupBody.preferredHeight);

        // active the popup
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates the popup.
    /// </summary>
    public void hidePopup()
    {
        // deactive the popup
        this.gameObject.SetActive(false);
        // set the popup heading to the default value
        popupHeading.text = default;
        // set the popup body to the default value
        popupBody.text = default;
    }
}
