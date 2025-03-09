// 


using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MouseSpawner : NetworkBehaviour
{
    public GameObject mousePrefab; // Mouse prefab
    public Transform mouseSpawnPoint; // Where mice spawn
    public List<Transform> movementTargets; // Targets for movement

    private void Start()
    {
        if (IsServer)
        {
            SpawnMouse();
        }
    }

    private void SpawnMouse()
    {
        if (!GameManager.Instance.IsGameActive()) return;
        
        if (mousePrefab == null || mouseSpawnPoint == null || movementTargets.Count == 0)
        {
            Debug.LogError("MouseSpawner is missing references!");
            return;
        }

        GameObject mouse = Instantiate(mousePrefab, mouseSpawnPoint.position, Quaternion.identity);
        mouse.GetComponent<NetworkObject>().Spawn();

        // Assign movement targets
        mouse.GetComponent<MouseBehaviour>().movementTargets = movementTargets;
    }
}
