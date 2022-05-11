using System;
using System.Linq;

[Serializable]
public class TerrainOptions : Options
{
    private enum TerrainSliderInputName { TerrainSize, TerrainExactHeight, TerrainRangeHeightMin, TerrainRangeHeightMax }

    private enum TerrainDropdownName { TerrainType, TerrainShape }

    private enum TerrainToggleOptionName { TerrainExactHeight, TerrainRangeHeight }

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
            setupToggle(toggles[i], toggleOptions[i]);
        }

    }

    public TerrainSettings createUserSettingsFromOptions()
    {
        // variables set here as this method is always called after submission of the options
        terrainType = (TerrainGenerator.TerrainType)dropdowns[((int)TerrainDropdownName.TerrainType)].value;
        bool heightRangeEnabled = toggles[(int)TerrainToggleOptionName.TerrainRangeHeight].isOn;

        int tSize = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainSize)].text);
        TerrainGenerator.TerrainShape tShape = (TerrainGenerator.TerrainShape)dropdowns[((int)TerrainDropdownName.TerrainShape)].value;

        int tMinHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMin)].text);
        int tMaxHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMax)].text);

        int tExactHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainExactHeight)].text);

        return new TerrainSettings(terrainType, heightRangeEnabled, tSize, tShape, tMinHeight, tMaxHeight, tExactHeight);
    }

    public void updateFields(TerrainSettings settings)
    {
        updateTerrainSizeField(settings.tSize);
        dropdowns[((int)TerrainDropdownName.TerrainType)].value = (int)settings.tType;

        if (settings.heightRangeEnabled)
        {
            toggles[(int)TerrainToggleOptionName.TerrainRangeHeight].isOn = true;

            sliders[((int)TerrainSliderInputName.TerrainRangeHeightMin)].value = settings.tMinHeight;
            sliders[((int)TerrainSliderInputName.TerrainRangeHeightMax)].value = settings.tMaxHeight;
        }
        else
        {
            toggles[(int)TerrainToggleOptionName.TerrainExactHeight].isOn = true;

            sliders[((int)TerrainSliderInputName.TerrainExactHeight)].value = settings.tExactHeight;
        }

       dropdowns[((int)TerrainDropdownName.TerrainShape)].value = (int)settings.tShape;
    }

    public void updateTerrainSizeField(int size)
    {
        sliders[((int)TerrainSliderInputName.TerrainSize)].value = size;
    }
}
