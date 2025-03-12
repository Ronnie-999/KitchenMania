using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanThrow : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Vector3 velocity;
    private bool isGrabbed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isGrabbed)
        {
            // Calculate velocity
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }
    }

    
    public void OnGrabbed()
    {
        isGrabbed = true;
        rb.isKinematic = true;
    }

        public void OnReleased(Transform handTransform)
    {
        isGrabbed = false;
        rb.isKinematic = false;

        Vector3 throwDirection = transform.forward;

        rb.velocity = throwDirection * velocity.magnitude * 2.5f; // Calculte throw velocity
        rb.angularVelocity = Vector3.zero; // keep from rotating

        // Add extra force to make dart fly
        rb.AddForce(handTransform.forward * 2.0f, ForceMode.Impulse);

        rb.AddTorque(Vector3.Cross(transform.forward, rb.velocity).normalized * 10f, ForceMode.Impulse);
    }


}
