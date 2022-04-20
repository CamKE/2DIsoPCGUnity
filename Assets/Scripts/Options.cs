using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Options
{
    [SerializeField]
    protected List<Slider> sliders;

    [SerializeField]
    protected List<InputField> inputFields;

    [SerializeField]
    protected List<Dropdown> dropdowns;

    [SerializeField]
    protected List<Toggle> toggles;

    [SerializeField]
    protected List<GameObject> toggleOptions;

    // all generators need the type of terrain
    protected static TerrainGenerator.terrainType terrainType;

    // common setup tasks to be done for sliders
    protected void setupSlider(Slider slider, InputField input, int minValue, int maxValue)
    {
        // limit slider to be between the min and max values
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        // set the input field to be equal to the sliders value
        input.text = slider.value.ToString("0");

        slider.onValueChanged.AddListener(delegate { updateSliderField(slider, input); });
        input.onEndEdit.AddListener(delegate { checkInputField(input, slider, minValue, maxValue); });
    }

    // common setup tasks to be done for dropdowns
    protected void setupDropdown(Dropdown dropdown, List<string> options)
    {
        // clear the dropdown
        dropdown.ClearOptions();
        // add the options
        dropdown.AddOptions(options);
    }

    public void updateSliderField(Slider slider, InputField input)
    {
        if (slider.value != float.Parse(input.text))
        {
            input.text = Math.Round(slider.value, MidpointRounding.AwayFromZero).ToString();
        }
    }

    /// <summary>
    /// Called anytime an input field has been edited to ensure the input is valid and the 
    /// slider is updated to reflect the input field.
    /// </summary>
    public void checkInputField(InputField input, Slider slider, int minValue, int maxValue)
    {
        if (input.text.Length != 0)
        {
            if (int.Parse(input.text) < minValue)
            {
                input.text = minValue.ToString("0");
            }
            else if (int.Parse(input.text) > maxValue)
            {
                input.text = maxValue.ToString("0");
            }

            slider.value = int.Parse(input.text);
        }

    }

    protected void setupToggle(Toggle toggle, GameObject option)
    {
        // make sure the options panel is initially in the correct state relative to the toggle state
        toggleOption(toggle, option);
        // add the toggle option to the on value change listener
        toggle.onValueChanged.AddListener(delegate { toggleOption(toggle, option); });
    }

    public void toggleOption(Toggle toggle, GameObject option)
    {
        option.SetActive(toggle.isOn ? true : false);
    }

    public TerrainGenerator.terrainType getTerrainType()
    {
        return terrainType;
    }
}
