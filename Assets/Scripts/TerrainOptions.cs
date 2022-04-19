using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TerrainOptions : Options
{
    private enum sliderInputName { terrainSize, terrainExactHeight, terrainRangeHeightMin, terrainRangeHeightMax }

    private enum dropdownName { terrainType, terrainShape }

    private enum toggleOptionName { terrainExactHeight, terrainRangeHeight }

    private bool rangeHeightEnabled;

    public void setupOptions()
    {
        // setup sliders
        int terrainSizeEnum = ((int)sliderInputName.terrainSize);
        setupSlider(sliders[terrainSizeEnum], inputFields[terrainSizeEnum], TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize);

        int terrainExactHeightEnum = ((int)sliderInputName.terrainExactHeight);
        for (int i = terrainExactHeightEnum; i < sliders.Count; i++)
        {
            setupSlider(sliders[i], inputFields[i], TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);
        }

        // setup dropdowns
        setupDropdown(dropdowns[((int)dropdownName.terrainType)], Enum.GetNames(typeof(TerrainGenerator.terrainType)).ToList());
        setupDropdown(dropdowns[((int)dropdownName.terrainShape)], Enum.GetNames(typeof(TerrainGenerator.terrainShape)).ToList());

        // setup toggles
        for (int i = 0; i < toggles.Count; i++)
        {
            setupToggle(toggles[i], toggleOptions[i]);
        }

    }

    public int getTerrainSize()
    {
        return int.Parse(inputFields[((int)sliderInputName.terrainSize)].text);
    }

    public TerrainGenerator.terrainType getTerrainType()
    {
        return (TerrainGenerator.terrainType)dropdowns[((int)dropdownName.terrainType)].value;
    }

    public int getTerrainMinHeight()
    {
        if (rangeHeightEnabled)
        {
            return int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMin)].text);
        } else
        { 
            return -1;
        }

    }

    public int getTerrainMaxHeight()
    {
        if (rangeHeightEnabled)
        {
            return int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMax)].text);
        }
        else
        {
            return -1;
        }
    }

    public int getTerrainExactHeight()
    {
        if (!rangeHeightEnabled)
        {
            return int.Parse(inputFields[((int)sliderInputName.terrainExactHeight)].text);
        }
        else
        {
            return -1;
        }
    }

    public TerrainGenerator.terrainShape getTerrainShape()
    {
        return (TerrainGenerator.terrainShape)dropdowns[((int)dropdownName.terrainShape)].value;
    }

    public bool heightRangeIsOnAndInvalid()
    {
        rangeHeightEnabled = toggles[(int)toggleOptionName.terrainRangeHeight].isOn;
        return rangeHeightEnabled && (int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMin)].text) > int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMax)].text));
    }

    public bool isRangedHeightEnabled()
    {
        return rangeHeightEnabled;
    }
}
