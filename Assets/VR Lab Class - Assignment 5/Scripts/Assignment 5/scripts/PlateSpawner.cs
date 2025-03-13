using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : NetworkBehaviour // Change to NetworkBehaviour
{
    public GameObject PlatePrefab;
    public Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }
    }

    [ServerRpc]
    public void RequestSpawnNewPlateServerRpc()
    {
        if (!IsServer) return; // Ensure this check works properly

        SpawnNewPlate();
    }

    public void SpawnNewPlate()
    {
        if (PlatePrefab == null)
        {
            Debug.LogWarning($"[{name}] PlatePrefab not assigned, cannot spawn!");
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[{name}] SpawnPoint is null, cannot spawn!");
            return;
        }

        GameObject newPlate = Instantiate(PlatePrefab, spawnPoint.position, spawnPoint.rotation);

        NetworkObject networkObject = newPlate.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Spawned Plate does not have a NetworkObject component!");
        }

        Rigidbody rb = newPlate.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
