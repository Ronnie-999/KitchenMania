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
        
        // Check if the colliding object is a pan
        PanThrow pan = collision.gameObject.GetComponent<PanThrow>();
        if (pan != null)
        {
            // Calculate impact force
            float impactForce = collision.relativeVelocity.magnitude;
            
            // Check if the impact is strong enough
            if (impactForce >= minImpactForce)
            {
                // The rat was hit hard enough, kill it
                DieServerRpc();
            }
        }
        if (!IsServer) return;
        
        // Check if the colliding object is a pan
        PumpkinThrow pumpkin = collision.gameObject.GetComponent<PumpkinThrow>();
        if (pumpkin != null)
        {
            // Calculate impact force
            float impactForce = collision.relativeVelocity.magnitude;
            
            // Check if the impact is strong enough
            if (impactForce >= minImpactForce)
            {
                // The rat was hit hard enough, kill it
                DieServerRpc();
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DieServerRpc()
    {
        // Spawn death effect if one is assigned
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            effect.GetComponent<NetworkObject>()?.Spawn();
        }
        
        Debug.Log("🐀 Rat was hit by a pan or pumpkin and died!");
        
        // Tell the spawner to create a new rat
        if (spawner != null)
        {
            spawner.RatKilled();
        }
        
        // Destroy the rat
        GetComponent<NetworkObject>().Despawn();
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