using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PumpkinSpawner : MonoBehaviour
{
    public GameObject PumpkinPrefab;
    public Transform spawnPoint; 

    private void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }
    }

public void SpawnNewPumpkin()
{
    if (PumpkinPrefab == null)
    {
        Debug.LogError($"[{name}] PumpkinPrefab not assigned, cannot spawn!");
        return;
    }
    if (spawnPoint == null)
    {
        Debug.LogError($"[{name}] SpawnPoint is null, cannot spawn!");
        return;
    }
    
    GameObject newPumpkin = Instantiate(PumpkinPrefab, spawnPoint.position, spawnPoint.rotation);
    if (newPumpkin.TryGetComponent<NetworkObject>(out NetworkObject netObj))
    {
        netObj.Spawn();  // Ensure it spawns properly over the network
    }

    Debug.Log($"Spawned new pumpkin at {spawnPoint.position}");

    Rigidbody rb = newPumpkin.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}

}