using System;
using System.Linq;

[Serializable]
public class RiverOptions : Options
{
    private enum RiverDropdownName { RiverAmount }

    private enum RiverToggleOptionName { RiverGeneration, RiverIntersection }

    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)RiverDropdownName.RiverAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)RiverToggleOptionName.RiverGeneration;
        setupToggle(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    public RiverSettings createUserSettingsFromOptions()
    {
        bool rGenerationEnabled = toggles[(int)RiverToggleOptionName.RiverGeneration].isOn;
        RiverGenerator.NumberOfRivers rNum = (RiverGenerator.NumberOfRivers)dropdowns[(int)RiverDropdownName.RiverAmount].value;
        bool intersectionsEnabled = toggles[(int)RiverToggleOptionName.RiverIntersection].isOn;

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
