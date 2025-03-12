using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatSpawner : NetworkBehaviour
{
    public GameObject ratPrefab;
    public Transform RatSpawnPoint;
    public List<Transform> movementTargets;
    
    // Add respawn configuration
    public bool autoRespawn = true;
    public float respawnDelay = 2.0f;
    
    // Optional: limit the number of rats that can be spawned
    public int maxRatsToSpawn = 0;  // 0 means unlimited
    private int ratsSpawned = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnRat();
        }
    }

    public void SpawnRat()
    {
        // Check if we've reached the maximum number of rats to spawn
        if (maxRatsToSpawn > 0 && ratsSpawned >= maxRatsToSpawn)
        {
            Debug.Log("[RatSpawner] Maximum number of rats reached.");
            return;
        }
        
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
        
        // Set reference to this spawner
        ratBehaviour.spawner = this;

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
        ratsSpawned++;
        Debug.Log("[RatSpawner] Rat spawned successfully. Total spawned: " + ratsSpawned);
    }
    
    // This method is called when a rat dies
    public void RatKilled()
    {
        if (!IsServer) return;
        
        if (autoRespawn)
        {
            Debug.Log("[RatSpawner] Rat killed. Respawning in " + respawnDelay + " seconds.");
            StartCoroutine(RespawnAfterDelay());
        }
    }
    
    // Coroutine to handle the respawn delay
    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnRat();
    }
}