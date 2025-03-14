using Unity.Netcode;
using UnityEngine;

public class ObjectAccessHandler : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new(writePerm: NetworkVariableWritePermission.Server);

    #endregion

    #region Custom Methods

    public bool RequestAccess()
    {
        if (!IsSpawned)
        {
            Debug.LogError($"[{name}] RequestAccess failed: Object is not spawned!");
            return false;
        }

        if (!isGrabbed.Value)
        {
            GrabObjectRpc(NetworkManager.LocalClientId);
            return true;
        }

        return false;
    }

    public void Release()
    {
        if (!IsSpawned)
        {
            Debug.LogError($"[{name}] Release failed: Object is not spawned!");
            return;
        }

        if (IsOwner)
            ReleaseObjectRpc();
    }

    #endregion

    #region RPCs

    [Rpc(SendTo.Server)]
private void GrabObjectRpc(ulong clientId)
{
    if (!IsSpawned || isGrabbed.Value)
    {
        Debug.LogError($"[{name}] GrabObjectRpc failed: Object is either not spawned or already grabbed.");
        return;
    }

    NetworkObject netObj = GetComponent<NetworkObject>();
    if (netObj == null)
    {
        Debug.LogError($"[{name}] GrabObjectRpc failed: NetworkObject is missing!");
        return;
    }

    // Change ownership before marking as grabbed
    netObj.ChangeOwnership(clientId);
    isGrabbed.Value = true;
}


    [Rpc(SendTo.Server)]
    private void ReleaseObjectRpc()
    {
        if (!IsSpawned)
        {
            Debug.LogError($"[{name}] ReleaseObjectRpc failed: Object is not spawned!");
            return;
        }

        if (GetComponent<NetworkObject>() == null)
        {
            Debug.LogError($"[{name}] ReleaseObjectRpc failed: NetworkObject is missing!");
            return;
        }

        isGrabbed.Value = false;
        GetComponent<NetworkObject>().RemoveOwnership();
    }

    #endregion
}
