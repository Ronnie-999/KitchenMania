using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PanSpawner : MonoBehaviour
{
    public GameObject PanPrefab;
    public Transform spawnPoint; 

    private void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }
    }

    public void SpawnNewPan()
{
    if (PanPrefab == null)
    {
        Debug.LogWarning($"[{name}] PanPrefab not assigned, cannot spawn!");
        return;
    }
    if (spawnPoint == null)
    {
        Debug.LogWarning($"[{name}] SpawnPoint is null, cannot spawn!");
        return;
    }

    GameObject newPan = Instantiate(PanPrefab, spawnPoint.position, spawnPoint.rotation);

    // Ensure the object has a NetworkObject component
    NetworkObject networkObject = newPan.GetComponent<NetworkObject>();
    if (networkObject != null)
    {
        networkObject.Spawn();  // Spawn the object over the network
    }
    else
    {
        Debug.LogError("Spawned Pan does not have a NetworkObject component!");
    }

    // Make the new Pan completely still
    Rigidbody rb = newPan.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
}