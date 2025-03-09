using Unity.Netcode;
using UnityEngine;

public class OwnershipHandler : NetworkBehaviour
{
    private NetworkVariable<bool> isOwned = new NetworkVariable<bool>(false);

    public bool RequestOwnership()
    {
        if (!isOwned.Value)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
            return true;
        }
        return false;
    }

    public void ReleaseOwnership()
    {
        if (IsOwner)
        {
            ReleaseOwnershipServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        if (!isOwned.Value)
        {
            isOwned.Value = true;
            GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }
    }

    [Rpc(SendTo.Server)]
    private void ReleaseOwnershipServerRpc()
    {
        isOwned.Value = false;
        GetComponent<NetworkObject>().RemoveOwnership();
    }
}
