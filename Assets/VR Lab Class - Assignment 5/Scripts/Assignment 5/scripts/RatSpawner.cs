using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatSpawner : NetworkBehaviour
{
    public GameObject ratPrefab;
    public Transform RatSpawnPoint;
    public List<Transform> movementTargets;
    
    public bool autoRespawn = true;
    public float respawnDelay = 2.0f;
    
    public int maxRatsToSpawn = 0;  // 0 means unlimited
    private int ratsSpawned = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            Debug.Log("[RatSpawner] Spawner spawned on the network. Spawning initial rat...");
            SpawnRat();
        }
        else
        {
            Debug.LogWarning("[RatSpawner] Not the server, cannot spawn rats.");
        }
    }

    public void SpawnRat()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[RatSpawner] SpawnRat() called on client. Ignoring.");
            return;
        }

        if (maxRatsToSpawn > 0 && ratsSpawned >= maxRatsToSpawn)
        {
            Debug.Log("[RatSpawner] Max rats reached. No more will spawn.");
            return;
        }

        if (ratPrefab == null)
        {
            Debug.LogError("[RatSpawner] ratPrefab is missing! Assign it in the Inspector.");
            return;
        }

        if (RatSpawnPoint == null)
        {
            Debug.LogWarning("[RatSpawner] RatSpawnPoint is missing! Using default Vector3.zero.");
        }

        Vector3 spawnPosition = RatSpawnPoint != null ? RatSpawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = RatSpawnPoint != null ? RatSpawnPoint.rotation : Quaternion.identity;

        GameObject rat = Instantiate(ratPrefab, spawnPosition, spawnRotation);

        if (rat == null)
        {
            Debug.LogError("[RatSpawner] Failed to instantiate rat!");
            return;
        }

        RatBehaviour ratBehaviour = rat.GetComponent<RatBehaviour>();
        if (ratBehaviour == null)
        {
            Debug.LogError("[RatSpawner] RatPrefab is missing RatBehaviour component! Destroying.");
            Destroy(rat);
            return;
        }

        ratBehaviour.movementTargets = movementTargets;
        ratBehaviour.spawner = this;

        NetworkObject netObj = rat.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[RatSpawner] RatPrefab is missing NetworkObject component! Destroying.");
            Destroy(rat);
            return;
        }

        netObj.Spawn();
        ratsSpawned++;
        Debug.Log($"[RatSpawner] Rat spawned successfully. Total spawned: {ratsSpawned}");
    }

    public void RatKilled()
    {
        if (!IsServer) return;

        if (autoRespawn)
        {
            Debug.Log("[RatSpawner] Rat killed. Respawning in " + respawnDelay + " seconds.");
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnRat();
    }
}
