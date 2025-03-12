using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.LogWarning($"[{name}] PumpkinPrefab not assigned, cannot spawn!");
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[{name}] SpawnPoint is null, cannot spawn!");
            return;
        }

        GameObject newPan = Instantiate(PanPrefab, spawnPoint.position, spawnPoint.rotation);

    // Make the new Pan completely still
    Rigidbody rb = newPan.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;   // If true, has a small movement. Looks better
    }
    }
}