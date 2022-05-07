using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RiverOptions : Options
{
    private enum RiverDropdownName { RiverAmount }

    private enum RiverToggleOptionName { RiverGeneration, RiverIntersection }

    public struct RiverSettings
    {
        readonly public TerrainGenerator.TerrainType tType;
        readonly public bool rGenerationEnabled;
        readonly public RiverGenerator.NumberOfRivers rNum;
        readonly public bool intersectionsEnabled;

        public RiverSettings(TerrainGenerator.TerrainType tType, bool rGenerationEnabled, RiverGenerator.NumberOfRivers rNum, bool intersectionsEnabled)
        {
            this.tType = tType;
            this.rGenerationEnabled = rGenerationEnabled;
            this.rNum = rNum;
            this.intersectionsEnabled = intersectionsEnabled;
        }
    }

    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)RiverDropdownName.RiverAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)RiverToggleOptionName.RiverGeneration;
        setupToggle(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    public RiverSettings createUserSettings()
    {
        bool rGenerationEnabled = toggles[(int)RiverToggleOptionName.RiverGeneration].isOn;
        RiverGenerator.NumberOfRivers rNum = (RiverGenerator.NumberOfRivers)dropdowns[(int)RiverDropdownName.RiverAmount].value;
        bool intersectionsEnabled = toggles[(int)RiverToggleOptionName.RiverIntersection].isOn;

        return new RiverSettings(terrainType, rGenerationEnabled, rNum, intersectionsEnabled);
    }
    

    public RiverSettings createRandomisedSettings()
    {
        bool rGenerationEnabled = UnityEngine.Random.value > 0.5f;
        RiverGenerator.NumberOfRivers rNum = (RiverGenerator.NumberOfRivers)UnityEngine.Random.Range(0, RiverGenerator.numberOfRiversCount);
        bool intersectionsEnabled = UnityEngine.Random.value > 0.5f;

        return new RiverSettings(terrainType, rGenerationEnabled, rNum, intersectionsEnabled);
    }

    public void updateFields(RiverSettings settings)
    {
        if (settings.rGenerationEnabled)
        {
            toggles[(int)RiverToggleOptionName.RiverGeneration].isOn = true;
            dropdowns[(int)RiverDropdownName.RiverAmount].value = (int)settings.rNum;
            toggles[(int)RiverToggleOptionName.RiverIntersection].isOn = settings.intersectionsEnabled;
        }
        else
        {
            toggles[(int)RiverToggleOptionName.RiverGeneration].isOn = false;
        }
    }
}
