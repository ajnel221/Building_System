using System.Collections.Generic;
using UnityEngine;

public class BuildingHolder : MonoBehaviour
{
    public BuildingObject Building;
    [SerializeField] private bool _buildingComplete = false;
    [SerializeField] private List<BuildingMaterialsNeeded> _needed;
    [SerializeField] private List<BuildingMaterialsNeeded> _inventory;
    [SerializeField] private GameObject[] _parts;

    private void Start()
    {   
        _needed = Building.BuildingSetup.SingleBuilingSetup.MaterialsNeeded;
        _inventory = new List<BuildingMaterialsNeeded>();

        foreach (var item in _needed)
        {
            var newItem = new BuildingMaterialsNeeded
            {
                BuildingMaterialAmountNeeded = item.BuildingMaterialAmountNeeded
            };
            _inventory.Add(newItem);
        }

        foreach (var item in _inventory)
        {
            item.BuildingMaterialAmountNeeded = 0;
        }

        // foreach (var item in _parts)
        // {
        //     item.SetActive(false);
        // }
    }

    private void FixedUpdate()
    {
        if (!_buildingComplete)
        {
            if (_inventory.Count == _needed.Count)
            {
                bool allMatch = true;
                
                for (int i = 0; i < _inventory.Count; i++)
                {
                    if (_inventory[i].BuildingMaterialAmountNeeded != _needed[i].BuildingMaterialAmountNeeded)
                    {
                        allMatch = false;
                        break;
                    }
                }

                if (allMatch)
                {
                    _buildingComplete = true;
                }
            }
        }
    }

    public void AddInventoryItem(int itemIndex, int amount)
    {
        // Ensure the index is within bounds
        if (itemIndex < 0 || itemIndex >= _inventory.Count)
        {
            Debug.LogError("Invalid item index.");
            return;
        }

        // Add the specified amount to the existing item
        if(_inventory[itemIndex].BuildingMaterialAmountNeeded < _needed[itemIndex].BuildingMaterialAmountNeeded)
        {
            _inventory[itemIndex].BuildingMaterialAmountNeeded += amount;
        }

        UpdateParts();
    }

    private void UpdateParts()
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            // Check if there's enough material for this inventory item
            int materialAmount = _inventory[i].BuildingMaterialAmountNeeded;
            for (int j = 0; j < materialAmount && j < _parts.Length; j++)
            {
                // Enable the corresponding part in _parts
                _parts[j].SetActive(true);
            }
        }
    }
}