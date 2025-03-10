using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatSpawner : NetworkBehaviour
{
    public GameObject ratPrefab;
    public Transform RatSpawnPoint;  // Assign in the Inspector
    public List<Transform> movementTargets;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the server should handle the spawning logic
        if (IsServer)
        {
            SpawnRat();
        }
    }

    public void SpawnRat()
    {
        // Check if ratPrefab is assigned
        if (ratPrefab == null)
        {
            Debug.LogError("[RatSpawner] ratPrefab is NOT assigned in the Inspector!");
            return;
        }

        // Check if RatSpawnPoint is assigned
        if (RatSpawnPoint == null)
        {
            Debug.LogError("[RatSpawner] RatSpawnPoint is NOT assigned in the Inspector!");
            return;
        }

        // Instantiate the rat at the spawn point
        GameObject rat = Instantiate(ratPrefab, RatSpawnPoint.position, RatSpawnPoint.rotation);

        // Ensure the instantiated rat has RatBehaviour
        RatBehaviour ratBehaviour = rat.GetComponent<RatBehaviour>();
        if (ratBehaviour == null)
        {
            Debug.LogError("[RatSpawner] RatPrefab is missing the RatBehaviour component!");
            Destroy(rat);
            return;
        }

        // Assign movement targets to the spawned rat
        ratBehaviour.movementTargets = movementTargets;

        // Ensure the instantiated rat has a NetworkObject component
        NetworkObject networkObject = rat.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("[RatSpawner] RatPrefab is missing the NetworkObject component!");
            Destroy(rat);
            return;
        }

        // Spawn the rat on the network
        networkObject.Spawn();
        Debug.Log("[RatSpawner] Rat spawned successfully.");
    }
}
