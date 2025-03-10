using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatBehaviour : NetworkBehaviour
{
    [HideInInspector] public List<Transform> movementTargets;  // List of targets the witch will fly to
    private int currentMovementTargetIdx = -1;  // Index of the current target

    public bool randomizeTargetOrder = false;  // Whether the flight targets should be randomized
    public float minTargetDistance = 1.5f;  // Minimum distance to consider the witch as having reached a target
    public float movementSpeed = 3f;  // Speed at which the witch moves
    public float rotationSpeed = 180f;  // Speed at which the witch rotates to face the target

    private void Start()
    {
        // Randomize flight targets if the flag is enabled
        if (randomizeTargetOrder)
        {
            ShuffleTargets();
        }

        // Start by setting the first target
        if (movementTargets.Count > 0)
        {
            currentMovementTargetIdx = 0;
        }
    }

    private void Update()
    {
        if (IsServer)  // Only the server handles the witch's movement
        {
            // Apply movement and rotation if the witch has valid targets
            if (movementTargets.Count > 0 && currentMovementTargetIdx >= 0)
            {
                ApplyMovement();
            }
        }
    }

    private void UpdateTarget()
    {
        // Move to the next target or loop back to the first one if at the end
        currentMovementTargetIdx = (currentMovementTargetIdx + 1) % movementTargets.Count;
    }

    private void ApplyMovement()
    {
        // Get the current target position
        Transform currentTarget = movementTargets[currentMovementTargetIdx];

        // Move towards the target
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, movementSpeed * Time.deltaTime);

        // Smoothly rotate towards the target
        Quaternion targetRotation = Quaternion.LookRotation(currentTarget.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Check if the witch has reached the target
        if (Vector3.Distance(transform.position, currentTarget.position) < minTargetDistance)
        {
            // Update to the next target
            UpdateTarget();
        }
    }

    // Optional method to randomize the order of flight targets
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
