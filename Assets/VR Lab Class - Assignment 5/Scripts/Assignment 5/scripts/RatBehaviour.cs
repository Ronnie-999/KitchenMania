using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatBehaviour : NetworkBehaviour
{
    [HideInInspector] public List<Transform> movementTargets;
    private int currentMovementTargetIdx = -1;

    // Reference to the spawner that created this rat
    [HideInInspector] public RatSpawner spawner;

    public bool randomizeTargetOrder = false;
    public float minTargetDistance = 1.5f;
    public float movementSpeed = 3f;
    public float rotationSpeed = 180f;
    public float minImpactForce = 2.0f;
    public GameObject deathEffect;

    private static int panHitCount = 0;
    private static int plateHitCount = 0;

    private void Start()
    {
        if (randomizeTargetOrder)
        {
            ShuffleTargets();
        }

        if (movementTargets.Count > 0)
        {
            currentMovementTargetIdx = 0;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (movementTargets.Count > 0 && currentMovementTargetIdx >= 0)
            {
                ApplyMovement();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
{
    if (!IsServer) return;

    float impactForce = collision.relativeVelocity.magnitude;
    
    if (collision.gameObject.TryGetComponent(out PanThrow pan) && impactForce >= minImpactForce)
    {
        panHitCount++;
        Debug.Log($"🥄 Pan Hits: {panHitCount}, 🍽 Plate Hits: {plateHitCount}, Score: {CalculateScore()}");

        if (GetComponent<NetworkObject>().IsSpawned) 
        {
            DieServerRpc();
        }
    }

    if (collision.gameObject.TryGetComponent(out PlateThrow plate) && impactForce >= minImpactForce)
    {
        plateHitCount++;
        Debug.Log($"🥄 Pan Hits: {panHitCount}, 🍽 Plate Hits: {plateHitCount}, Score: {CalculateScore()}");

        if (GetComponent<NetworkObject>().IsSpawned) 
        {
            DieServerRpc();
        }
    }
}


    private int CalculateScore()
    {
        return (panHitCount * 1) + (plateHitCount * 2);
    }

    [ServerRpc(RequireOwnership = false)]
private void DieServerRpc()
{
    if (!IsServer) return; // Ensure only the server runs this

    NetworkObject networkObject = GetComponent<NetworkObject>();
    
    if (networkObject == null || !networkObject.IsSpawned)
    {
        Debug.LogWarning("⚠️ Attempted to call DieServerRpc on an unspawned NetworkObject!");
        return;
    }

    // Spawn death effect if assigned
    if (deathEffect != null)
    {
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        NetworkObject effectNetworkObject = effect.GetComponent<NetworkObject>();
        if (effectNetworkObject != null)
        {
            effectNetworkObject.Spawn();
        }
    }

    Debug.Log("🐀 Rat was hit and died!");

    // Tell the spawner to create a new rat
    if (spawner != null)
    {
        spawner.RatKilled();
    }

    // Despawn the rat safely
    networkObject.Despawn();
    Destroy(gameObject, 0.1f);
}

    private void UpdateTarget()
    {
        currentMovementTargetIdx = (currentMovementTargetIdx + 1) % movementTargets.Count;
    }

    private void ApplyMovement()
    {
        Transform currentTarget = movementTargets[currentMovementTargetIdx];
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, movementSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(currentTarget.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget.position) < minTargetDistance)
        {
            UpdateTarget();
        }
    }

    private void ShuffleTargets()
    {
        for (int i = 0; i < movementTargets.Count; i++)
        {
            Transform temp = movementTargets[i];
            int randomIndex = Random.Range(i, movementTargets.Count);
            movementTargets[i] = movementTargets[randomIndex];
            movementTargets[randomIndex] = temp;
        }
    }
}
