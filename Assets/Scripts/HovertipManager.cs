using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

// Credit to Comp-3 Interative for the tutorial
public class HoverTipManager : MonoBehaviour
{
    public TextMeshProUGUI tipText;
    public RectTransform tipWindow;

    public static Action<string, Vector2> onMouseHover;
    public static Action onMouseLoseFocus;

    private void Start()
    {
        hideTip();
    }

    private void OnEnable()
    {
        onMouseHover += showTip;
        onMouseLoseFocus += hideTip;
    }

    private void OnDisable()
    {
        onMouseHover -= showTip;
        onMouseLoseFocus -= hideTip;
    }

    private void showTip(string tip, Vector2 mousePos)
    {
        tipText.text = tip;
        tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 200 ? 200 : tipText.preferredWidth, tipText.preferredHeight);
        Debug.Log(tipWindow.sizeDelta);
        tipWindow.gameObject.SetActive(true);
        tipWindow.transform.position = new Vector2(mousePos.x + tipWindow.sizeDelta.x * 0.6f, mousePos.y);
    }

    private void hideTip()
    {
        tipText.text = default;
        tipWindow.gameObject.SetActive(false);
    }
}
