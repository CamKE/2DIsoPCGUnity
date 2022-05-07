using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LakeOptions : Options
{
    private enum LakeDropdownName { LakeAmount, LakeMaxSize }

    private enum LakeToggleOptionName { LakeGeneration }

    public struct LakeSettings
    {
        readonly public TerrainGenerator.TerrainType tType;
        readonly public bool lGenerationEnabled;
        readonly public LakeGenerator.NumberOfLakes lNum;
        readonly public LakeGenerator.MaxLakeSize lMaxSize;

        public LakeSettings(TerrainGenerator.TerrainType tType, bool lGenerationEnabled, LakeGenerator.NumberOfLakes lNum, LakeGenerator.MaxLakeSize lMaxSize)
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
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeAmount)], Enum.GetNames(typeof(LakeGenerator.NumberOfLakes)).ToList());
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeMaxSize)], Enum.GetNames(typeof(LakeGenerator.MaxLakeSize)).ToList());

        // setup toggle
        int lakeGenerationEnum = (int)LakeToggleOptionName.LakeGeneration;
        setupToggle(toggles[lakeGenerationEnum], toggleOptions[lakeGenerationEnum]);
    }

    public LakeSettings createUserSettings()
    {
        bool lGenerationEnabled = toggles[(int)LakeToggleOptionName.LakeGeneration].isOn;
        LakeGenerator.NumberOfLakes lNum = (LakeGenerator.NumberOfLakes)dropdowns[(int)LakeDropdownName.LakeAmount].value;
        LakeGenerator.MaxLakeSize lMaxSize = (LakeGenerator.MaxLakeSize)dropdowns[(int)LakeDropdownName.LakeMaxSize].value;

        return new LakeSettings(terrainType, lGenerationEnabled, lNum, lMaxSize);
    }
    
    public LakeSettings createRandomisedSettings()
    {
        bool lGenerationEnabled = UnityEngine.Random.value > 0.5f;

        LakeGenerator.NumberOfLakes lNum = (LakeGenerator.NumberOfLakes)UnityEngine.Random.Range(0, LakeGenerator.numberOfLakesCount);

        LakeGenerator.MaxLakeSize lMaxSize = (LakeGenerator.MaxLakeSize)UnityEngine.Random.Range(0, LakeGenerator.maxLakeSizeCount);

        return new LakeSettings(terrainType, lGenerationEnabled, lNum, lMaxSize);
    }

    public void updateFields(LakeSettings settings)
    {
        if (settings.lGenerationEnabled)
        {
            toggles[(int)LakeToggleOptionName.LakeGeneration].isOn = true;
            dropdowns[(int)LakeDropdownName.LakeAmount].value = (int)settings.lNum;
            dropdowns[(int)LakeDropdownName.LakeMaxSize].value = (int)settings.lMaxSize;
        } else
        {
            toggles[(int)LakeToggleOptionName.LakeGeneration].isOn = false;
        }
    }

}
