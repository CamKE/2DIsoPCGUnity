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

    public struct TerrainSettings
    {
        readonly public TerrainGenerator.terrainType tType;
        readonly public int tSize;
        readonly public int tMinHeight, tMaxHeight;
        readonly public int tExactHeight;
        readonly public TerrainGenerator.terrainShape tShape;
        readonly public bool heightRangedEnabled;

        public TerrainSettings(int tSize, TerrainGenerator.terrainType tType, int tMinHeight, int tMaxHeight, TerrainGenerator.terrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tMinHeight = tMinHeight;
            this.tMaxHeight = tMaxHeight;
            this.tShape = tShape;
            tExactHeight = -1;
            heightRangedEnabled = true;
        }

        public TerrainSettings(int tSize, TerrainGenerator.terrainType tType, int tExactHeight, TerrainGenerator.terrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tExactHeight = tExactHeight;
            this.tShape = tShape;
            tMinHeight = -1;
            tMaxHeight = -1;
            heightRangedEnabled = false;
        }
    }

    public void setupUIElements()
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

    public TerrainSettings createUserSettings()
    {
        int tSize = int.Parse(inputFields[((int)sliderInputName.terrainSize)].text);
        TerrainGenerator.terrainShape tShape = (TerrainGenerator.terrainShape)dropdowns[((int)dropdownName.terrainShape)].value;

        if (rangeHeightEnabled)
        {
            int tMinHeight = int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMin)].text);
            int tMaxHeight = int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMax)].text);

            return new TerrainSettings(tSize, terrainType, tMinHeight, tMaxHeight, tShape);
        }
        else
        {
            int tExactHeight = int.Parse(inputFields[((int)sliderInputName.terrainExactHeight)].text);

            return new TerrainSettings(tSize, terrainType, tExactHeight, tShape);
        }
    }

    public bool heightRangeIsOnAndInvalid()
    {
        // variables set here as this method is always called after submission of the options
        terrainType = (TerrainGenerator.terrainType)dropdowns[((int)dropdownName.terrainType)].value;
        rangeHeightEnabled = toggles[(int)toggleOptionName.terrainRangeHeight].isOn;
        return rangeHeightEnabled && (int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMin)].text) > int.Parse(inputFields[((int)sliderInputName.terrainRangeHeightMax)].text));
    }
}
