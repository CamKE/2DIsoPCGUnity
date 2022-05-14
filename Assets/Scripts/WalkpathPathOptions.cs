using System;
using System.Linq;

[Serializable]
public class WalkpathPathOptions : Options
{
    private enum WalkpathDropdownName { WalkpathAmount }

    private enum WalkpathToggleOptionName { WalkpathGeneration, WalkpathIntersection }

    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)WalkpathDropdownName.WalkpathAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup walkpath generation toggle
        int walkpathGenerationEnum = (int)WalkpathToggleOptionName.WalkpathGeneration;
        setupToggleWithOption(toggles[walkpathGenerationEnum], toggleOptions[walkpathGenerationEnum]);
    }

    public WalkpathSettings createUserSettingsFromOptions()
    {
        bool wGenerationEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn;
        WalkpathGenerator.NumberOfWalkpaths wNum = (WalkpathGenerator.NumberOfWalkpaths)dropdowns[(int)WalkpathDropdownName.WalkpathAmount].value;
        bool intersectionsEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn;

        return new WalkpathSettings(terrainType, wGenerationEnabled, wNum, intersectionsEnabled);
    }

    public void updateFields(WalkpathSettings settings)
    {
        if (settings.wGenerationEnabled)
        {
            toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn = true;
            dropdowns[(int)WalkpathDropdownName.WalkpathAmount].value = (int)settings.wNum;
            toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn = settings.intersectionsEnabled;
        }
        else
        {
            toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn = false;
        }
    }
}
