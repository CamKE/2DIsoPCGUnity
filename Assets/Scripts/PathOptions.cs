using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class WalkpathPathOptions : Options
{
    private enum WalkpathDropdownName { WalkpathDAmount }

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
        setupDropdown(dropdowns[((int)WalkpathDropdownName.WalkpathDAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)WalkpathToggleOptionName.WalkpathGeneration;
        setupToggle(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    public WalkpathSettings createUserSettings()
    {
        bool rGenerationEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathGeneration].isOn;
        WalkpathGenerator.NumberOfWalkpaths wNum = (WalkpathGenerator.NumberOfWalkpaths)dropdowns[(int)WalkpathDropdownName.WalkpathDAmount].value;
        bool intersectionsEnabled = toggles[(int)WalkpathToggleOptionName.WalkpathIntersection].isOn;

        return new WalkpathSettings(terrainType, rGenerationEnabled, wNum, intersectionsEnabled);
    }
}
