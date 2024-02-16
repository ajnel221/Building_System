using UnityEngine;
using System;

public class Connector : MonoBehaviour
{
    public ConnectorPosition connectorPosition;
    public SelectedBuildType connectorParentType;

    public bool isConnectedToFloor = false;
    public bool isConnectedToWall = false;
    public bool canConnectTo = true;
    public bool canConnectToFloor = true;
    public bool canConnectToWall = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnectedToFloor ? (isConnectedToWall ? Color.red : Color.blue) : (!isConnectedToWall ? Color.green : Color.yellow);
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x / 2f);
    }

    public void UpdateConnectors(bool rootCall = false)
    {
        Collider[] _colliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2f);

        isConnectedToFloor = !canConnectToFloor;
        isConnectedToWall = !canConnectToWall;

        foreach (Collider item in _colliders)
        {
            if(item.GetInstanceID() == GetComponent<Collider>().GetInstanceID())
            {
                continue;
            }

            if(!item.gameObject.activeInHierarchy)
            {
                continue;
            }

            if(item.gameObject.layer == gameObject.layer)
            {
                Connector _foundConnector = item.GetComponent<Connector>();

                if(_foundConnector.connectorParentType == SelectedBuildType.floor)
                {
                    isConnectedToFloor = true;
                }

                if(_foundConnector.connectorParentType == SelectedBuildType.wall)
                {
                    isConnectedToWall = true;
                }

                if(rootCall)
                {
                    _foundConnector.UpdateConnectors();
                }
            }
        }

        canConnectTo = true;

        if(isConnectedToFloor && isConnectedToWall)
        {
            canConnectTo = false;
        }
    }
}

[Serializable]
public enum ConnectorPosition
{
    left,
    right,
    top,
    bottom,
}