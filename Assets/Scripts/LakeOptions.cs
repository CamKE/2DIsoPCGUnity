using System;
using System.Linq;

/// <summary>
/// Contains the options for lake generation.
/// </summary>
[Serializable]
public class LakeOptions : Options
{
    // the names of the lake dropdowns
    private enum LakeDropdownName { LakeAmount, LakeMaxSize }
    // the names of the lake toggle and options
    private enum LakeToggleOptionName { LakeGeneration }

    /// <summary>
    /// Setup the lake generation ui elements.
    /// </summary>
    public void setupUIElements()
    {
        // setup dropdowns
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeAmount)], Enum.GetNames(typeof(LakeGenerator.NumberOfLakes)).ToList());
        setupDropdown(dropdowns[((int)LakeDropdownName.LakeMaxSize)], Enum.GetNames(typeof(LakeGenerator.MaxLakeSize)).ToList());

        // setup toggle
        int lakeGenerationEnum = (int)LakeToggleOptionName.LakeGeneration;
        setupToggle(toggles[lakeGenerationEnum], toggleOptions[lakeGenerationEnum]);
    }

    /// <summary>
    /// Creates the user lake settings from the options given.
    /// </summary>
    /// <returns>The lake settings from the options.</returns>
    public LakeSettings createUserSettingsFromOptions()
    {
        // get the options from the ui
        bool lGenerationEnabled = toggles[(int)LakeToggleOptionName.LakeGeneration].isOn;
        LakeGenerator.NumberOfLakes lNum = (LakeGenerator.NumberOfLakes)dropdowns[(int)LakeDropdownName.LakeAmount].value;
        LakeGenerator.MaxLakeSize lMaxSize = (LakeGenerator.MaxLakeSize)dropdowns[(int)LakeDropdownName.LakeMaxSize].value;

        // creating the settings
        return new LakeSettings(terrainType, lGenerationEnabled, lNum, lMaxSize);
    }

    /// <summary>
    /// Update the ui elements/fields based on the settings used. Used after random generation.
    /// </summary>
    /// <param name="settings">The settings used.</param>
    public void updateFields(LakeSettings settings)
    {
        // update the lake generation toggle
        toggles[(int)LakeToggleOptionName.LakeGeneration].isOn = settings.lGenerationEnabled;
        // if lake generation is on
        if (settings.lGenerationEnabled)
        {
            // update the ui elements
            dropdowns[(int)LakeDropdownName.LakeAmount].value = (int)settings.lNum;
            dropdowns[(int)LakeDropdownName.LakeMaxSize].value = (int)settings.lMaxSize;
        }
    }

}
