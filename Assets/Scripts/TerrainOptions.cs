using System;
using System.Linq;

/// <summary>
/// Used to manage user interface elements for the terrain generator.
/// </summary>
[Serializable]
public class TerrainOptions : Options
{
    // names of terrain sliders and inputs
    private enum TerrainSliderInputName { TerrainSize, TerrainExactHeight, TerrainRangeHeightMin, TerrainRangeHeightMax }

    // names of terrain dropdowns
    private enum TerrainDropdownName { TerrainType, TerrainShape }

    // names of terrain toggles and options
    private enum TerrainToggleOptionName { TerrainExactHeight, TerrainRangeHeight }

    /// <summary>
    /// Setup the user interface elements for the terrain options.
    /// </summary>
    public void setupUIElements()
    {
        // setup sliders
        int terrainSizeEnum = ((int)TerrainSliderInputName.TerrainSize);
        setupSlider(sliders[terrainSizeEnum], inputFields[terrainSizeEnum], TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);

        int terrainExactHeightEnum = ((int)TerrainSliderInputName.TerrainExactHeight);
        for (int i = terrainExactHeightEnum; i < sliders.Count; i++)
        {
            setupSlider(sliders[i], inputFields[i], TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }

        // setup dropdowns
        setupDropdown(dropdowns[((int)TerrainDropdownName.TerrainType)], Enum.GetNames(typeof(TerrainGenerator.TerrainType)).ToList());
        setupDropdown(dropdowns[((int)TerrainDropdownName.TerrainShape)], Enum.GetNames(typeof(TerrainGenerator.TerrainShape)).ToList());

        // setup toggles
        for (int i = 0; i < toggles.Count; i++)
        {
            setupToggleWithOption(toggles[i], toggleOptions[i]);
        }
    }

    /// <summary>
    /// Create the terrain settings from the terrain options.
    /// </summary>
    /// <returns>The terrain setttings.</returns>
    public TerrainSettings createUserSettingsFromOptions()
    {
        // retieve the options and creating the settings from it

        terrainType = (TerrainGenerator.TerrainType)dropdowns[((int)TerrainDropdownName.TerrainType)].value;
        bool heightRangeEnabled = toggles[(int)TerrainToggleOptionName.TerrainRangeHeight].isOn;

        int tSize = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainSize)].text);
        TerrainGenerator.TerrainShape tShape = (TerrainGenerator.TerrainShape)dropdowns[((int)TerrainDropdownName.TerrainShape)].value;

        int tMinHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMin)].text);
        int tMaxHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMax)].text);

        int tExactHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainExactHeight)].text);

        return new TerrainSettings(terrainType, heightRangeEnabled, tSize, tShape, tMinHeight, tMaxHeight, tExactHeight);
    }

    /// <summary>
    /// Update the user interface options with the settings used for terrain generation
    /// </summary>
    /// <param name="settings">The settings used for terrain generation.</param>
    public void updateFields(TerrainSettings settings)
    {
        // update the terrain size
        updateTerrainSizeField(settings.tSize);

        // update the terrain type dropdown
        dropdowns[((int)TerrainDropdownName.TerrainType)].value = (int)settings.tType;
        // update the terrain shape dropdown
        dropdowns[((int)TerrainDropdownName.TerrainShape)].value = (int)settings.tShape;

        // if height range option is on
        if (settings.heightRangeEnabled)
        {
            // update the toggle
            toggles[(int)TerrainToggleOptionName.TerrainRangeHeight].isOn = true;

            // set the min and max sliders
            sliders[((int)TerrainSliderInputName.TerrainRangeHeightMin)].value = settings.tMinHeight;
            sliders[((int)TerrainSliderInputName.TerrainRangeHeightMax)].value = settings.tMaxHeight;
        }
        else
        {
            // update the exact height toggle
            toggles[(int)TerrainToggleOptionName.TerrainExactHeight].isOn = true;

            // set the exact height slider
            sliders[((int)TerrainSliderInputName.TerrainExactHeight)].value = settings.tExactHeight;
        }
    }

    /// <summary>
    /// Update the terrain size slider in the user interface with the setting used.
    /// </summary>
    /// <param name="size">The actual size of the level generated.</param>
    public void updateTerrainSizeField(int size)
    {
        sliders[((int)TerrainSliderInputName.TerrainSize)].value = size;
    }
}
