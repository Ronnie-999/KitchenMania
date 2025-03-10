using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableObject : XRGrabInteractable
{
    protected override void Awake()
    {
        base.Awake();
        if (!GetComponent<Rigidbody>())
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false; // Allows physics interaction
        }
    }
}
