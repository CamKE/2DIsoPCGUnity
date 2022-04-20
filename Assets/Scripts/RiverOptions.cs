using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RiverOptions : Options
{
    private enum dropdownName { riverAmount }

    private enum toggleOptionName { riverGeneration, riverIntersection, riverBridges }

    public struct RiverSettings
    {
        readonly public TerrainGenerator.terrainType tType;
        readonly public bool rGenerationEnabled;
        readonly public RiverGenerator.numRivers rNum;
        readonly public bool intersectionsEnabled;
        readonly public bool bridgesEnabled;

        public RiverSettings(TerrainGenerator.terrainType tType, bool rGenerationEnabled, RiverGenerator.numRivers rNum, bool intersectionsEnabled, bool bridgesEnabled)
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
        setupDropdown(dropdowns[((int)dropdownName.riverAmount)], Enum.GetNames(typeof(RiverGenerator.numRivers)).ToList());

        // setup river generation toggle
        int riverGenerationEnum = (int)toggleOptionName.riverGeneration;
        setupToggle(toggles[riverGenerationEnum], toggleOptions[riverGenerationEnum]);
    }

    public RiverSettings createUserSettings()
    {
        bool rGenerationEnabled = toggles[(int)toggleOptionName.riverGeneration].isOn;
        RiverGenerator.numRivers rNum = (RiverGenerator.numRivers)dropdowns[(int)dropdownName.riverAmount].value;
        bool intersectionsEnabled = toggles[(int)toggleOptionName.riverIntersection].isOn;
        bool bridgesEnabled = toggles[(int)toggleOptionName.riverBridges].isOn;

        return new RiverSettings(terrainType, rGenerationEnabled, rNum, intersectionsEnabled, bridgesEnabled);
    }
}
