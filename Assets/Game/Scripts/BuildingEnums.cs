using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingMaterials
{
    public enum BuildingMaterialsEnum
    {
        Log,
        Stone,
    }
}

[Serializable]
public class BuildingTypes
{
    public enum BuildingTypesEnum
    {
        WoodFloor,
        WoodWall,
        WoodRoof,
        StoneFloor,
        StoneWall,
        StoneRoof,
    }
}

[Serializable]
public class BuildingMaterialsNeeded
{
    public BuildingMaterials.BuildingMaterialsEnum BuildingMaterial;
    public int BuildingMaterialAmountNeeded;
}

[Serializable]
public class BuilingSetup
{
    public BuildingTypes.BuildingTypesEnum BuildingType;
    public List<BuildingMaterialsNeeded> MaterialsNeeded = new List<BuildingMaterialsNeeded>();

    [Header("Prefab")]
    public GameObject PrefabObject;
}

[Serializable]
public class BuildingRequirements
{
    public BuilingSetup[] BuilingSetups;
}

[Serializable]
public class BuildingRequirement
{
    public BuilingSetup SingleBuilingSetup;
}