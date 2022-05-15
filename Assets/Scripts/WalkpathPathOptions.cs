using System;
using System.Linq;

/// <summary>
/// Used to manage user interface elements for the walkpath generator.
/// </summary>
[Serializable]
public class WalkpathPathOptions : Options
{
    // names of walkpath dropdowns
    private enum WalkpathDropdownName { WalkpathAmount }

    // names of walkpath toggles and options
    private enum WalkpathToggleOptionName { WalkpathGeneration, WalkpathIntersection }

    /// <summary>
    /// Setup the user interface elements for the walkpath options.
    /// </summary>
    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)WalkpathDropdownName.WalkpathAmount)], Enum.GetNames(typeof(WalkpathGenerator.NumberOfWalkpaths)).ToList());

        // setup walkpath generation toggle
        int walkpathGenerationEnum = (int)WalkpathToggleOptionName.WalkpathGeneration;
        setupToggleWithOption(toggles[walkpathGenerationEnum], toggleOptions[walkpathGenerationEnum]);
    }

    /// <summary>
    /// Create the walkpath settings from the walkpath options.
    /// </summary>
    /// <returns>The walkpath setttings.</returns>
    public WalkpathSettings createUserSettingsFromOptions()
    {
        // retieve the options and creating the settings from it

        bool wGenerationEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn;
        WalkpathGenerator.NumberOfWalkpaths wNum = (WalkpathGenerator.NumberOfWalkpaths)dropdowns[(int)WalkpathDropdownName.WalkpathAmount].value;
        bool intersectionsEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn;

        return new WalkpathSettings(terrainType, wGenerationEnabled, wNum, intersectionsEnabled);
    }

    /// <summary>
    /// Update the user interface options with the settings used for walkpath generation.
    /// </summary>
    /// <param name="settings">The settings used for walkpath generation.</param>
    public void updateFields(WalkpathSettings settings)
    {
        // update the toggle
        toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn = settings.wGenerationEnabled;

        // if walkpath generation is on
        if (settings.wGenerationEnabled)
        {
            // update the fields
            dropdowns[(int)WalkpathDropdownName.WalkpathAmount].value = (int)settings.wNum;
            toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn = settings.intersectionsEnabled;
        }
    }
}
