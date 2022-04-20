using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LakeOptions : Options
{
    private enum dropdownName { lakeAmount, lakeMaxSize }

    private enum toggleOptionName { lakeGeneration }

    public struct LakeSettings
    {
        readonly public TerrainGenerator.terrainType tType;
        readonly public bool lGenerationEnabled;
        readonly public LakeGenerator.numLakes lNum;
        readonly public LakeGenerator.maxLakeSize lMaxSize;

        public LakeSettings(TerrainGenerator.terrainType tType, bool lGenerationEnabled, LakeGenerator.numLakes lNum, LakeGenerator.maxLakeSize lMaxSize)
        {
            this.tType = tType;
            this.lGenerationEnabled = lGenerationEnabled;
            this.lNum = lNum;
            this.lMaxSize = lMaxSize;
        }
    }

    public void setupUIElements()
    {
        // setup dropdowns
        setupDropdown(dropdowns[((int)dropdownName.lakeAmount)], Enum.GetNames(typeof(LakeGenerator.numLakes)).ToList());
        setupDropdown(dropdowns[((int)dropdownName.lakeMaxSize)], Enum.GetNames(typeof(LakeGenerator.maxLakeSize)).ToList());

        // setup toggle
        int lakeGenerationEnum = (int)toggleOptionName.lakeGeneration;
        setupToggle(toggles[lakeGenerationEnum], toggleOptions[lakeGenerationEnum]);
    }

    public LakeSettings createUserSettings()
    {
        bool lGenerationEnabled = toggles[(int)toggleOptionName.lakeGeneration].isOn;
        LakeGenerator.numLakes lNum = (LakeGenerator.numLakes)dropdowns[(int)dropdownName.lakeAmount].value;
        LakeGenerator.maxLakeSize lMaxSize = (LakeGenerator.maxLakeSize)dropdowns[(int)dropdownName.lakeMaxSize].value;

        return new LakeSettings(terrainType, lGenerationEnabled, lNum, lMaxSize);
    }
}
