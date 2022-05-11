using System;
using System.Linq;

[Serializable]
public class LakeOptions : Options
{
    private enum LakeDropdownName { LakeAmount, LakeMaxSize }

    private enum LakeToggleOptionName { LakeGeneration }

    public void setupUIElements()
    {
        // setup dropdowns
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeAmount)], Enum.GetNames(typeof(LakeGenerator.NumberOfLakes)).ToList());
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeMaxSize)], Enum.GetNames(typeof(LakeGenerator.MaxLakeSize)).ToList());

        // setup toggle
        int lakeGenerationEnum = (int)LakeToggleOptionName.LakeGeneration;
        setupToggle(toggles[lakeGenerationEnum], toggleOptions[lakeGenerationEnum]);
    }

    public LakeSettings createUserSettingsFromOptions()
    {
        bool lGenerationEnabled = toggles[(int)LakeToggleOptionName.LakeGeneration].isOn;
        LakeGenerator.NumberOfLakes lNum = (LakeGenerator.NumberOfLakes)dropdowns[(int)LakeDropdownName.LakeAmount].value;
        LakeGenerator.MaxLakeSize lMaxSize = (LakeGenerator.MaxLakeSize)dropdowns[(int)LakeDropdownName.LakeMaxSize].value;

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
