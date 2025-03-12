using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinThrow : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Vector3 velocity;
    private bool isGrabbed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevents missed collisions
    }

    void Update()
    {
        if (isGrabbed)
        {
            // Calculate velocity based on movement
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }
    }

    
    public void OnGrabbed()
    {
        isGrabbed = true;
        rb.isKinematic = true; // Disable physics when grabbed
        rb.velocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;
        lastPosition = transform.position;
    }

    public void OnReleased(Transform handTransform)
    {
        isGrabbed = false;
        rb.isKinematic = false; // Re-enable physics

        if (velocity.magnitude < 0.1f) 
        {
            velocity = handTransform.forward * 3.0f; // Give a fallback velocity if throw is weak
        }

        rb.velocity = velocity; // Apply velocity
        rb.angularVelocity = Vector3.zero;

        // Extra throw force for realism
        rb.AddForce(handTransform.forward * 2.0f, ForceMode.Impulse);

        // Apply torque for rotation effect
        rb.AddTorque(Vector3.Cross(Vector3.up, rb.velocity).normalized * 10f, ForceMode.Impulse);

        Debug.Log($"🔄 Pan thrown with velocity: {rb.velocity}");
    }
}
