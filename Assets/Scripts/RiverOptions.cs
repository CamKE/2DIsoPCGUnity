using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RiverOptions : Options
{
    private enum RiverDropdownName { RiverAmount }

    private enum RiverToggleOptionName { RiverGeneration, RiverIntersection, RiverBridges }

    public struct RiverSettings
    {
        readonly public TerrainGenerator.TerrainType tType;
        readonly public bool rGenerationEnabled;
        readonly public RiverGenerator.NumberOfRivers rNum;
        readonly public bool intersectionsEnabled;
        readonly public bool bridgesEnabled;

        public RiverSettings(TerrainGenerator.TerrainType tType, bool rGenerationEnabled, RiverGenerator.NumberOfRivers rNum, bool intersectionsEnabled, bool bridgesEnabled)
        {
            this.tType = tType;
            this.rGenerationEnabled = rGenerationEnabled;
            this.rNum = rNum;
            this.intersectionsEnabled = intersectionsEnabled;
            this.bridgesEnabled = bridgesEnabled;
        }
    }

    public void setupUIElements()
    {
        // setup dropdown
        setupDropdown(dropdowns[((int)RiverDropdownName.RiverAmount)], Enum.GetNames(typeof(RiverGenerator.NumberOfRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)RiverToggleOptionName.RiverGeneration;
        setupToggle(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    public RiverSettings createUserSettings()
    {
        bool rGenerationEnabled = toggles[(int)RiverToggleOptionName.RiverGeneration].isOn;
        RiverGenerator.NumberOfRivers rNum = (RiverGenerator.NumberOfRivers)dropdowns[(int)RiverDropdownName.RiverAmount].value;
        bool intersectionsEnabled = toggles[(int)RiverToggleOptionName.RiverIntersection].isOn;
        bool bridgesEnabled = toggles[(int)RiverToggleOptionName.RiverBridges].isOn;

        return new RiverSettings(terrainType, rGenerationEnabled, rNum, intersectionsEnabled, bridgesEnabled);
    }
}
