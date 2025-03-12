using UnityEngine;
using Unity.Netcode;

public class PlateThrow : NetworkBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float velocityMultiplier = 1.5f;
    [SerializeField] private float minThrowVelocity = 1.0f;
    [SerializeField] private float extraForwardForce = 2.0f;
    [SerializeField] private float torqueMultiplier = 1.0f;

    private Rigidbody rb;
    private Vector3 previousHandPosition;
    private Vector3 previousHandPositionLastFrame;
    private Vector3[] velocityFrames = new Vector3[5];
    private int velocityFrameIndex = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError($"[{name}] Rigidbody component not found on this object!");
            enabled = false;
            return;
        }

        // Enable continuous collision detection
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        // Initialize velocity frames
        for (int i = 0; i < velocityFrames.Length; i++)
        {
            velocityFrames[i] = Vector3.zero;
        }
    }

    public void OnGrabbed(Transform handTransform)
    {
        if (!IsOwner) return;

        // Reset angular velocity when grabbed
        rb.angularVelocity = Vector3.zero;

        // Initialize position tracking for velocity calculation
        previousHandPosition = handTransform.position;
        previousHandPositionLastFrame = previousHandPosition;

        // Reset velocity frames
        for (int i = 0; i < velocityFrames.Length; i++)
        {
            velocityFrames[i] = Vector3.zero;
        }
        velocityFrameIndex = 0;
    }

    private void Update()
    {
        // This is used to track hand movement over time
        if (rb.isKinematic)
        {
            // Only execute if we're being held
            TrackHandMovementServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrackHandMovementServerRpc()
    {
        // This method would track hand movement to compute velocity
        // In a networked context, this would be called via ServerRpc
    }

    public void OnReleased(Transform handTransform, Vector3 releaseVelocity)
    {
        if (!IsOwner) return;

        Vector3 throwVelocity = releaseVelocity * velocityMultiplier;

        // Ensure minimum velocity
        if (throwVelocity.magnitude < minThrowVelocity)
        {
            throwVelocity = handTransform.forward * minThrowVelocity;
        }

        ApplyThrowServerRpc(throwVelocity, handTransform.forward, handTransform.up);
    }


    private Vector3 CalculateAverageVelocity(Vector3 currentHandPosition)
    {
        // Current frame velocity
        Vector3 frameVelocity = (currentHandPosition - previousHandPositionLastFrame) / Time.deltaTime;

        // Add to circular buffer
        velocityFrames[velocityFrameIndex] = frameVelocity;
        velocityFrameIndex = (velocityFrameIndex + 1) % velocityFrames.Length;

        // Calculate average
        Vector3 averageVelocity = Vector3.zero;
        for (int i = 0; i < velocityFrames.Length; i++)
        {
            averageVelocity += velocityFrames[i];
        }
        averageVelocity /= velocityFrames.Length;

        // Update position tracking
        previousHandPositionLastFrame = currentHandPosition;

        return averageVelocity;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyThrowServerRpc(Vector3 throwVelocity, Vector3 handForward, Vector3 handUp)
    {
        rb.isKinematic = false;  // Ensure physics is enabled
        rb.useGravity = true;
        rb.velocity = throwVelocity;
        rb.AddForce(handForward * extraForwardForce, ForceMode.Impulse);

        Vector3 torqueDirection = Vector3.Cross(Vector3.up, throwVelocity.normalized);
        if (torqueDirection.magnitude < 0.1f)
        {
            torqueDirection = Vector3.Cross(handUp, throwVelocity.normalized);
        }

        float torquePower = throwVelocity.magnitude * torqueMultiplier;
        rb.AddTorque(torqueDirection * torquePower, ForceMode.Impulse);

        SyncThrowEffectClientRpc(rb.velocity, rb.angularVelocity);
    }


    [ClientRpc]
    private void SyncThrowEffectClientRpc(Vector3 velocity, Vector3 angularVelocity)
    {
        if (IsOwner) return; // Owner already applied these values

        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
    }

    // This should be called from the VirtualHand script when the plate is grabbed
    public void SetKinematic(bool isKinematic)
    {
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
            rb.useGravity = !isKinematic;

            // Reset velocities when grabbed
            if (isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}