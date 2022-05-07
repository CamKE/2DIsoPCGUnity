using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class WalkpathPathOptions : Options
{
    private enum WalkpathDropdownName { WalkpathAmount }

    private enum WalkpathToggleOptionName { WalkpathGeneration, WalkpathIntersection }

    public struct WalkpathSettings
    {
        readonly public TerrainGenerator.TerrainType tType;
        readonly public bool wGenerationEnabled;
        readonly public WalkpathGenerator.NumberOfWalkpaths wNum;
        readonly public bool intersectionsEnabled;

        public WalkpathSettings(TerrainGenerator.TerrainType tType, bool wGenerationEnabled, WalkpathGenerator.NumberOfWalkpaths wNum, bool intersectionsEnabled)
        {
            this.tType = tType;
            this.wGenerationEnabled = wGenerationEnabled;
            this.wNum = wNum;
            this.intersectionsEnabled = intersectionsEnabled;
        }
    }

    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)WalkpathDropdownName.WalkpathAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup walkpath generation toggle
        int walkpathGenerationEnum = (int)WalkpathToggleOptionName.WalkpathGeneration;
        setupToggle(toggles[walkpathGenerationEnum], toggleOptions[walkpathGenerationEnum]);
    }

    public WalkpathSettings createUserSettings()
    {
        bool wGenerationEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn;
        WalkpathGenerator.NumberOfWalkpaths wNum = (WalkpathGenerator.NumberOfWalkpaths)dropdowns[(int)WalkpathDropdownName.WalkpathAmount].value;
        bool intersectionsEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn;

        return new WalkpathSettings(terrainType, wGenerationEnabled, wNum, intersectionsEnabled);
    }

    public WalkpathSettings createRandomisedSettings()
    {
        bool wGenerationEnabled = UnityEngine.Random.value > 0.5f;

        WalkpathGenerator.NumberOfWalkpaths wNum = (WalkpathGenerator.NumberOfWalkpaths)UnityEngine.Random.Range(0, WalkpathGenerator.numberOfWalkpathsCount);

        bool intersectionsEnabled = UnityEngine.Random.value > 0.5f;

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
