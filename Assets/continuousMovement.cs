using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour
{
    public float speed = 1;
    public XRNode inputSource;
    public Transform headTransform; // Reference to the camera (head)

    private Vector2 inputAxis;
    private CharacterController character;

    void Start()
    {
        character = GetComponent<CharacterController>();

        // If no head transform is assigned, try to find the main camera (VR headset)
        if (headTransform == null && Camera.main != null)
        {
            headTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
    }

    private void FixedUpdate()
    {
        if (headTransform == null) return;

        // Get the forward and right directions relative to the head's yaw
        Vector3 forward = headTransform.forward;
        Vector3 right = headTransform.right;

        // Flatten them to prevent movement in the Y-axis (prevent flying up/down)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction
        Vector3 direction = (forward * inputAxis.y + right * inputAxis.x).normalized;

        // Move the character
        character.Move(direction * Time.fixedDeltaTime * speed);
    }
}
