using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingObject", menuName = "Building_System/BuildingObject", order = 0)]
public class BuildingObject : ScriptableObject
{
    public BuildingRequirement BuildingSetup;
}