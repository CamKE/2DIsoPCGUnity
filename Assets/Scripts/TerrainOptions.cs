using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TerrainOptions : Options
{
    private enum TerrainSliderInputName { TerrainSize, TerrainExactHeight, TerrainRangeHeightMin, TerrainRangeHeightMax }

    private enum TerrainDropdownName { TerrainType, TerrainShape }

    private enum TerrainToggleOptionName { TerrainExactHeight, TerrainRangeHeight }

    public class TerrainSettings
    {
        readonly public TerrainGenerator.TerrainType tType;
        public int tSize { get; private set; }
        readonly public int tMinHeight, tMaxHeight;
        readonly public int tExactHeight;
        readonly public TerrainGenerator.TerrainShape tShape;
        readonly public bool heightRangedEnabled;

        public TerrainSettings(int tSize, TerrainGenerator.TerrainType tType, int tMinHeight, int tMaxHeight, TerrainGenerator.TerrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tMinHeight = tMinHeight;
            this.tMaxHeight = tMaxHeight;
            this.tShape = tShape;
            tExactHeight = -1;
            heightRangedEnabled = true;
        }

        public TerrainSettings(int tSize, TerrainGenerator.TerrainType tType, int tExactHeight, TerrainGenerator.TerrainShape tShape)
        {
            this.tSize = tSize;
            this.tType = tType;
            this.tExactHeight = tExactHeight;
            this.tShape = tShape;
            tMinHeight = -1;
            tMaxHeight = -1;
            heightRangedEnabled = false;
        }

        public bool heightRangeIsOnAndInvalid()
        {
            return heightRangedEnabled && tMinHeight >= tMaxHeight;
        }

        public void updateTerrainSize(int tSize)
        {
            this.tSize = tSize;
        }
    }

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

    public TerrainSettings createUserSettings()
    {
        // variables set here as this method is always called after submission of the options
        terrainType = (TerrainGenerator.TerrainType)dropdowns[((int)TerrainDropdownName.TerrainType)].value;
        bool heightRangedEnabled = toggles[(int)TerrainToggleOptionName.TerrainRangeHeight].isOn;

        int tSize = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainSize)].text);
        TerrainGenerator.TerrainShape tShape = (TerrainGenerator.TerrainShape)dropdowns[((int)TerrainDropdownName.TerrainShape)].value;

        if (heightRangedEnabled)
        {
            int tMinHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMin)].text);
            int tMaxHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainRangeHeightMax)].text);

            return new TerrainSettings(tSize, terrainType, tMinHeight, tMaxHeight, tShape);
        }
        else
        {
            int tExactHeight = int.Parse(inputFields[((int)TerrainSliderInputName.TerrainExactHeight)].text);

            return new TerrainSettings(tSize, terrainType, tExactHeight, tShape);
        }
    }

    public TerrainSettings createRandomisedSettings()
    {
        // variables set here as this method is always called after submission of the options
        terrainType = (TerrainGenerator.TerrainType)UnityEngine.Random.Range(0, TerrainGenerator.terrainTypeCount);

        bool heightRangedEnabled = UnityEngine.Random.value > 0.5f;

        // TerrainGenerator.terrainMinSize to TerrainGenerator.terrainMaxSize
        int tSize = UnityEngine.Random.Range(TerrainGenerator.terrainMinSize, TerrainGenerator.terrainMaxSize+1);

        TerrainGenerator.TerrainShape tShape = (TerrainGenerator.TerrainShape)UnityEngine.Random.Range(0, TerrainGenerator.terrainShapeCount);

        if (heightRangedEnabled)
        {
            // TerrainGenerator.terrainMinHeight to TerrainGenerator.terrainMaxHeight-1
            int tMinHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight);

            // Greater than tMinHeight, up to the terrainMaxHeight
            int tMaxHeight = UnityEngine.Random.Range(tMinHeight+1, TerrainGenerator.terrainMaxHeight+1);

            return new TerrainSettings(tSize, terrainType, tMinHeight, tMaxHeight, tShape);
        }
        else
        {
            int tExactHeight = UnityEngine.Random.Range(TerrainGenerator.terrainMinHeight, TerrainGenerator.terrainMaxHeight+1);

            return new TerrainSettings(tSize, terrainType, tExactHeight, tShape);
        }
    }

    public void updateFields(TerrainSettings settings)
    {
        updateTerrainSizeField(settings.tSize);
        dropdowns[((int)TerrainDropdownName.TerrainType)].value = (int)settings.tType;

        if (settings.heightRangedEnabled)
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
