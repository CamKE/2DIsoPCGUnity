using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to manage user interface elements for a generator.
/// </summary>
[Serializable]
public class Options
{
    /// <summary>
    /// The sliders elements.
    /// </summary>
    [SerializeField]
    protected List<Slider> sliders;

    /// <summary>
    /// The input field elements.
    /// </summary>
    [SerializeField]
    protected List<InputField> inputFields;

    /// <summary>
    /// The dropdown elements.
    /// </summary>
    [SerializeField]
    protected List<Dropdown> dropdowns;

    /// <summary>
    /// The toggle elements.
    /// </summary>
    [SerializeField]
    protected List<Toggle> toggles;

    /// <summary>
    /// The game objects containing options to be enabled/disabled by
    /// a corresponding toggle.
    /// </summary>
    [SerializeField]
    protected List<GameObject> toggleOptions;

    /// <summary>
    /// All options need to know the terrain type.
    /// </summary>
    protected static TerrainGenerator.TerrainType terrainType;

    /// <summary>
    /// Common setup tasks to be done for sliders.
    /// </summary>
    /// <param name="slider">The slider to be setup.</param>
    /// <param name="input">The corresponding input field for the slider. Shows the slider value and can modify it.</param>
    /// <param name="minValue">The minimum possible value for the slider.</param>
    /// <param name="maxValue">The maximum possible value for the slider.</param>
    protected void setupSlider(Slider slider, InputField input, int minValue, int maxValue)
    {
        // limit slider to be between the min and max values
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        // set the input field to be equal to the sliders value
        input.text = slider.value.ToString("0");

        // add the listeners to the slider and input field events.
        slider.onValueChanged.AddListener(delegate { updateInputField(slider, input); });
        input.onEndEdit.AddListener(delegate { updateSlider(input, slider); });
    }

    /// <summary>
    /// Common setup tasks to be done for dropdowns.
    /// </summary>
    /// <param name="dropdown">The dropdown to be setup.</param>
    /// <param name="options">The options to be added to the dropdown element.</param>
    protected void setupDropdown(Dropdown dropdown, List<string> options)
    {
        // clear the dropdown default options
        dropdown.ClearOptions();
        // add the options
        dropdown.AddOptions(options);
    }

    /// <summary>
    /// Called by the sliders onValueChanged event to update the corresponding input field
    /// with the sliders value when it is changed.
    /// </summary>
    /// <param name="slider">The slider which called the method.</param>
    /// <param name="input">The slider's corresponding input field.</param>
    public void updateInputField(Slider slider, InputField input)
    {
        // if the input field is modified, then the update slider method is triggered. Once it is 
        // called and the slider is updated to the input field value, then this method is triggered by
        // the sliders onvaluechanged event. At that point, the values will already be the same, so we
        // do not need to update it.

        // enusre update only if the values are not already the same
        if (slider.value != float.Parse(input.text))
        {
            input.text = slider.value.ToString();
        }
    }

    /// <summary>
    /// Called by the input fields onEndEdit event to ensure the input is valid and the slider 
    /// matches the input field value.
    /// </summary>
    /// <param name="input">The input field which called the method.</param>
    /// <param name="slider">The input field's corresponding slider.</param>
    public void updateSlider(InputField input, Slider slider)
    {
        // as long as the input field text is not empty
        if (input.text.Length != 0)
        {
            // ensure the input field value between the minimum and maximum slider value
            input.text = Mathf.Clamp(int.Parse(input.text), (int)slider.minValue, (int)slider.maxValue).ToString();
        } 
        else
        // otherwise
        {
            // set the input field value to be the minimum slider value
            input.text = slider.minValue.ToString("0");
        }

        // update the slider with the input fields new value
        slider.value = int.Parse(input.text);
    }

    /// <summary>
    /// Common setup tasks to be done for toggles which enabled/disable a set of options.
    /// </summary>
    /// <param name="toggle">The toggle to be setup.</param>
    /// <param name="optionPanel">The options panel whose active state is affected by the toggle.</param>
    protected void setupToggleWithOption(Toggle toggle, GameObject optionPanel)
    {
        // make sure the options panel is initially in the correct state relative to the toggle state
        toggleOption(toggle, optionPanel);
        // add the toggle option to the on value change listener
        toggle.onValueChanged.AddListener(delegate { toggleOption(toggle, optionPanel); });
    }

    /// <summary>
    /// Called by the toggle's on value changed event when the toggle is enabled or disabled. Changes the
    /// state of the toggle's corresponding options panel.
    /// </summary>
    /// <param name="toggle">The toggle which called the method.</param>
    /// <param name="optionPanel">The options panel whose active state is affected by the toggle.</param>
    public void toggleOption(Toggle toggle, GameObject optionPanel)
    {
        // set the option panel state to match the toggle state
        optionPanel.SetActive(toggle.isOn);
    }
}
