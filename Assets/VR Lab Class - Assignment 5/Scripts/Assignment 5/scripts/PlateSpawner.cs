using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlateSpawner : MonoBehaviour
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

        // Ensure the object has a NetworkObject component
        NetworkObject networkObject = newPlate.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();  // Spawn the object over the network
        }
        else
        {
            Debug.LogError("Spawned Plate does not have a NetworkObject component!");
        }

        // Make the new Pan completely still
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