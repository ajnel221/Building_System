using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> _floorObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> _wallObjects = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType _currentBuildType;
    [SerializeField] private LayerMask _connectorLayer;

    [Header("Destroy Settings")]
    [SerializeField] private bool _isDestroying = false;
    private Transform _lastHitDestroyTransform;
    private List<Material> _lastHitMaterials = new List<Material>();

    [Header("Ghost Settings")]
    [SerializeField] private Material _ghostMaterialValid;
    [SerializeField] private Material _ghostMaterialInvalid;
    [SerializeField] private float _connectorOverlaypRadius = 1f;
    [SerializeField] private float _maxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool _isBuilding = false;
    [SerializeField] private int _currentBuildingIndex;
    private GameObject _ghostBuildGameObject;
    private bool _isGhostInvalidPosition = false;
    private Transform _modelParent = null;

    [Header("UI")]
    [SerializeField] private GameObject _buildingUI;
    [SerializeField] private TMP_Text _destroyText;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBuildingUI(!_buildingUI.activeInHierarchy);
        }

        // if(Input.GetKeyDown(KeyCode.B))
        // {
        //     _isBuilding = !_isBuilding;
        // }

        // if(Input.GetKeyDown(KeyCode.V))
        // {
        //     _isDestroying = !_isDestroying;
        // }

        if(_isBuilding && !_isDestroying)
        {
            GhostBuild();

            if(Input.GetMouseButtonDown(0))
            {
                PlaceBuild();
            }
        }
        else if(_ghostBuildGameObject)
        {
            Destroy(_ghostBuildGameObject);
            _ghostBuildGameObject = null;
        }

        if(_isDestroying)
        {
            GhostDestroy();

            if(Input.GetMouseButtonDown(0))
            {
                DestroyBuild();
            }
        }
    }

    private void GhostBuild()
    {
        GameObject _currentBuild = GetCurrentBuild();
        CreateGhostPrefab(_currentBuild);

        MoveGhostPrefabToRaycast();
        CheckBuilValidility();
    }

    private void CreateGhostPrefab(GameObject _currentBuild)
    {
        if(_ghostBuildGameObject == null)
        {
            _ghostBuildGameObject = Instantiate(_currentBuild);

            _modelParent = _ghostBuildGameObject.transform.GetChild(0);

            GhostifyModel(_modelParent, _ghostMaterialValid);
            GhostifyModel(_ghostBuildGameObject.transform);
        }
    }

    private void MoveGhostPrefabToRaycast()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;

        if(Physics.Raycast(_ray, out _hit))
        {
            _ghostBuildGameObject.transform.position = _hit.point;
        }
    }

    private void CheckBuilValidility()
    {
        Collider[] _colliders = Physics.OverlapSphere(_ghostBuildGameObject.transform.position, _connectorOverlaypRadius, _connectorLayer);

        if(_colliders.Length > 0)
        {
            GhostConnectBuild(_colliders);
        }
        else
        {
            GhostSeperateBuild();

            if(_isGhostInvalidPosition)
            {
                Collider[] _overlapColliders = Physics.OverlapBox(_ghostBuildGameObject.transform.position, new Vector3(2f, 2f, 2f), _ghostBuildGameObject.transform.rotation);

                foreach (Collider item in _overlapColliders)
                {
                    if(item.gameObject != _ghostBuildGameObject && item.transform.root.CompareTag("Buildables"))
                    {
                        GhostifyModel(_modelParent, _ghostMaterialInvalid);
                        _isGhostInvalidPosition = false;
                        return;
                    }
                }
            }
        }
    }

    private void GhostConnectBuild(Collider[] _colliders)
    {
        Connector _bestConnector = null;

        foreach (Collider _collider in _colliders)
        {
            Connector _connector = _collider.GetComponent<Connector>();

            if(_connector.canConnectTo)
            {
                _bestConnector = _connector;
                break;
            }
        }

        if(_bestConnector == null || _currentBuildType == SelectedBuildType.floor && _bestConnector.isConnectedToFloor || _currentBuildType == SelectedBuildType.wall && _bestConnector.isConnectedToWall)
        {
            GhostifyModel(_modelParent, _ghostMaterialInvalid);
            _isGhostInvalidPosition = false;
            return;
        }

        SnapGhostPrefabToConnector(_bestConnector);
    }

    private void SnapGhostPrefabToConnector(Connector _connector)
    {
        Transform _ghostConnector = FindSnapConnector(_connector.transform, _ghostBuildGameObject.transform.GetChild(1));
        _ghostBuildGameObject.transform.position = _connector.transform.position - (_ghostConnector.position - _ghostBuildGameObject.transform.position);

        if(_currentBuildType == SelectedBuildType.wall)
        {
            Quaternion _newRotation = _ghostBuildGameObject.transform.rotation;
            _newRotation.eulerAngles = new Vector3(_newRotation.eulerAngles.x, _connector.transform.rotation.eulerAngles.y, _newRotation.eulerAngles.z);
            _ghostBuildGameObject.transform.rotation = _newRotation;
        }

        GhostifyModel(_modelParent, _ghostMaterialValid);
        _isGhostInvalidPosition = true;
    }

    private void GhostSeperateBuild()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;

        if(Physics.Raycast(_ray, out _hit))
        {
            if(_currentBuildType == SelectedBuildType.wall)
            {
                GhostifyModel(_modelParent, _ghostMaterialInvalid);
                _isGhostInvalidPosition = false;
                return;
            }

            if(Vector3.Angle(_hit.normal, Vector3.up) < _maxGroundAngle)
            {
                GhostifyModel(_modelParent, _ghostMaterialValid);
                _isGhostInvalidPosition = true;
            }
            else
            {
                GhostifyModel(_modelParent, _ghostMaterialInvalid);
                _isGhostInvalidPosition = false;
            }
        }
    }

    private Transform FindSnapConnector(Transform _snapConnector, Transform _ghostConnectorParent)
    {
        ConnectorPosition _oppositeConnectorTag = GetOppositePosition(_snapConnector.GetComponent<Connector>());

        foreach (Connector _connector in _ghostConnectorParent.GetComponentsInChildren<Connector>())
        {
            if(_connector.connectorPosition == _oppositeConnectorTag)
            {
                return _connector.transform;
            }
        }

        return null;
    }

    private ConnectorPosition GetOppositePosition(Connector _connector)
    {
        ConnectorPosition _position = _connector.connectorPosition;

        if(_currentBuildType == SelectedBuildType.wall && _connector.connectorParentType == SelectedBuildType.floor)
        {
            return ConnectorPosition.bottom;
        }

        if(_currentBuildType == SelectedBuildType.floor && _connector.connectorParentType == SelectedBuildType.wall && _connector.connectorPosition == ConnectorPosition.top)
        {
            if(_connector.transform.root.rotation.y == 0)
            {
                return GetConnectorClosestToPlayer(true);
            }
            else
            {
                return GetConnectorClosestToPlayer(false);   
            }
        }

        switch (_position)
        {
            case ConnectorPosition.left:
                return ConnectorPosition.right;

            case ConnectorPosition.right:
                return ConnectorPosition.left;

            case ConnectorPosition.top:
                return ConnectorPosition.bottom;

            case ConnectorPosition.bottom:
                return ConnectorPosition.top;

            default:
                return ConnectorPosition.bottom;
        }
    }

    private ConnectorPosition GetConnectorClosestToPlayer(bool _topBottom)
    {
        Transform _cameraTransform = Camera.main.transform;

        if(_topBottom)
        {
            return _cameraTransform.position.z >= _ghostBuildGameObject.transform.position.z ? ConnectorPosition.bottom : ConnectorPosition.top;
        }
        else
        {
            return _cameraTransform.position.x >= _ghostBuildGameObject.transform.position.x ? ConnectorPosition.left : ConnectorPosition.right;
        }
    }

    private void GhostifyModel(Transform _modelParent, Material _ghostMaterial = null)
    {
        if(_ghostMaterial != null)
        {
            foreach (MeshRenderer _meshRenderer in _modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                _meshRenderer.material = _ghostMaterial;
            }
        }
        else
        {
            foreach (Collider _modelCollider in _modelParent.GetComponentsInChildren<Collider>())
            {
                _modelCollider.enabled = false;
            }
        }
    }

    private GameObject GetCurrentBuild()
    {
        switch (_currentBuildType)
        {
            case SelectedBuildType.floor:
                return _floorObjects[_currentBuildingIndex];

            case SelectedBuildType.wall:
                return _wallObjects[_currentBuildingIndex];
        }

        return null;
    }

    private void PlaceBuild()
    {
        if(_ghostBuildGameObject != null && _isGhostInvalidPosition)
        {
            GameObject _newBuild = Instantiate(GetCurrentBuild(), _ghostBuildGameObject.transform.position, _ghostBuildGameObject.transform.rotation);

            Destroy(_ghostBuildGameObject);
            _ghostBuildGameObject = null;

            //_isBuilding = false;

            foreach (Connector _connector in _newBuild.GetComponentsInChildren<Connector>())
            {
                _connector.UpdateConnectors();
            }
        }
    }

    private void GhostDestroy()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit _hit;

        if(Physics.Raycast(_ray, out _hit))
        {
            if(_hit.transform.root.CompareTag("Buildables"))
            {
                if(!_lastHitDestroyTransform)
                {
                    _lastHitDestroyTransform = _hit.transform.root;

                    _lastHitMaterials.Clear();

                    foreach (MeshRenderer item in _lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
                    {
                        _lastHitMaterials.Add(item.material);
                    }

                    GhostifyModel(_lastHitDestroyTransform.GetChild(0), _ghostMaterialInvalid);
                }
                else if(_hit.transform.root != _lastHitDestroyTransform)
                {
                    ResetLastHitDestroyTransform();
                }
            }
            else if(_lastHitDestroyTransform)
            {
                ResetLastHitDestroyTransform();
            }
        }
    }

    private void ResetLastHitDestroyTransform()
    {
        int _counter = 0;

        foreach (MeshRenderer item in _lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
        {
            item.material= _lastHitMaterials[_counter];
            _counter++;
        }

        _lastHitDestroyTransform = null;
    }

    private void DestroyBuild()
    {
        if(_lastHitDestroyTransform)
        {
            foreach (Connector _connector in _lastHitDestroyTransform.GetComponentsInChildren<Connector>())
            {
                _connector.gameObject.SetActive(false);
                _connector.UpdateConnectors(true);
            }

            Destroy(_lastHitDestroyTransform.gameObject);

            DestroyBuildingToggle(true);
            _lastHitDestroyTransform = null;
        }
    }

    public void ToggleBuildingUI(bool _active)
    {
        _isBuilding = false;
        _buildingUI.SetActive(_active);

        // Cursor.visible = _active;
        // Cursor.lockState = _active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void DestroyBuildingToggle(bool _fromScript = false)
    {
        if(_fromScript)
        {
            _isDestroying = false;
            _destroyText.text = "Destroy Off";
            _destroyText.color = Color.green;
        }
        else
        {
            _isDestroying = !_isDestroying;
            _destroyText.text = _isDestroying ? "Destroy On" : "Destroy Off";
            _destroyText.color = _isDestroying ? Color.red : Color.green;
            ToggleBuildingUI(false);
        }
    }

    public void ChangeBuildType(string _selectedBuildType)
    {
        if(System.Enum.TryParse(_selectedBuildType, out SelectedBuildType _result))
        {
            _currentBuildType = _result;
        }
        else
        {
            Debug.Log("Build Type Does NOT Exist.");
        }
    }

    public void StartBuilding(int _builIndex)
    {
        _currentBuildingIndex = _builIndex;
        ToggleBuildingUI(false);

        _isBuilding = true;
    }
}

[Serializable]
public enum SelectedBuildType
{
    floor,
    wall,
}