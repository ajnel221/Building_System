using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] BuildingHolder _holder;
    private GameObject _test;
    private bool _isMoving = true;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            _holder.AddInventoryItem(0,1);
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            _holder.AddInventoryItem(1,1);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            GhostBuildObject();
        }
        // else if(Input.GetKeyUp(KeyCode.B))
        // {
        //     Destroy(_test);
        // }

        if(_test != null)
        {
            MoveGhostPrefabToRaycast();
        }

        if(Input.GetMouseButtonDown(0))
        {
            _isMoving = false;
        }
    }

    private void GhostBuildObject()
    {
        GameObject _currentObject = _holder.Building.BuildingSetup.SingleBuilingSetup.PrefabObject;
        _test = Instantiate(_currentObject);
        _isMoving = true;;
    }

    private void MoveGhostPrefabToRaycast()
    {
        if(_isMoving)
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit _hit;

            if(Physics.Raycast(_ray, out _hit))
            {
                _test.transform.position = _hit.point;
            }
        }
    }
}