using System;
using System.Linq;

/// <summary>
/// Used to manage user interface elements for the river generator.
/// </summary>
[Serializable]
public class RiverOptions : Options
{
    // names of river dropdown(s)
    private enum RiverDropdownName { RiverAmount }

    // names of river toggles and options
    private enum RiverToggleOptionName { RiverGeneration, RiverIntersection }

    /// <summary>
    /// Setup the user interface elements for the river options.
    /// </summary>
    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)RiverDropdownName.RiverAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)RiverToggleOptionName.RiverGeneration;
        setupToggleWithOption(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    /// <summary>
    /// Create the river settings from the river options.
    /// </summary>
    /// <returns>The river setttings.</returns>
    public RiverSettings createUserSettingsFromOptions()
    {
        // retieve the options and creating the settings from it

        bool rGenerationEnabled = toggles[(int)RiverToggleOptionName.RiverGeneration].isOn;
        RiverGenerator.NumberOfRivers rNum = (RiverGenerator.NumberOfRivers)dropdowns[(int)RiverDropdownName.RiverAmount].value;
        bool intersectionsEnabled = toggles[(int)RiverToggleOptionName.RiverIntersection].isOn;

        return new RiverSettings(terrainType, rGenerationEnabled, rNum, intersectionsEnabled);
    }

    /// <summary>
    /// Update the user interface options with the settings used.
    /// </summary>
    /// <param name="settings">The settings used for river generation.</param>
    public void updateFields(RiverSettings settings)
    {
        // update the toggle
        toggles[(int)RiverToggleOptionName.RiverGeneration].isOn = settings.rGenerationEnabled;

        // if river generation is on
        if (settings.rGenerationEnabled)
        {
            // update the fields
            dropdowns[(int)RiverDropdownName.RiverAmount].value = (int)settings.rNum;
            toggles[(int)RiverToggleOptionName.RiverIntersection].isOn = settings.intersectionsEnabled;
        }
    }
}
