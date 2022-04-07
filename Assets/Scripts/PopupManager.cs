using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [SerializeField]
    private Text popupHeading;

    [SerializeField]
    private Text popupBody;

    public void showPopup(string messageHeading, string messageBody)
    {
        popupHeading.text = messageHeading;
        popupBody.text = messageBody;
        this.gameObject.SetActive(true);
    }

    public void hidePopup()
    {
        this.gameObject.SetActive(false);
        popupHeading.text = default;
        popupBody.text = default;
    }
}
