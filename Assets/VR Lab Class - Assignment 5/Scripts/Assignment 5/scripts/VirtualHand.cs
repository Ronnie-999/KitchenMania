using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualHand : MonoBehaviour
{
    #region Enum

    private enum VirtualHandMode
    {
        Snap,
        Reparenting,
        NoReparenting
    }

    #endregion
    
    #region Member Variables

    public InputActionProperty toggleModeAction;
    [SerializeField] private VirtualHandMode virtualHandMode = VirtualHandMode.Snap;

    public InputActionProperty grabAction;
    public HandCollider handCollider;

    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    private bool canGrab
    {
        get
        {
            if (handCollider == null)
            {
                Debug.LogError("handCollider is null in canGrab!");
                return false;
            }

            if (!handCollider.isColliding)
            {
                Debug.Log("canGrab = false: handCollider is not colliding with anything.");
                return false;
            }

            if (handCollider.collidingObject == null)
            {
                Debug.Log("canGrab = false: collidingObject is null.");
                return false;
            }

            ObjectAccessHandler accessHandler = handCollider.collidingObject.GetComponent<ObjectAccessHandler>();
            if (accessHandler == null)
            {
                Debug.LogWarning($"Skipping grab: {handCollider.collidingObject.name} is not a grabbable object.");
                return false;
            }
            if (handCollider.collidingObject != null)
            {
                Debug.Log($"HandCollider detected: {handCollider.collidingObject.name}");
            }

            bool accessGranted = accessHandler.RequestAccess();

            return accessGranted;
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if (!GetComponentInParent<NetworkObject>().IsOwner)
        {
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        if (grabAction.action.WasPressedThisFrame())
            Debug.Log("Grab button is pressed.");

        if (toggleModeAction.action.WasPressedThisFrame())
            virtualHandMode = (VirtualHandMode)(((int)virtualHandMode + 1) % 3);

        switch (virtualHandMode)
        {
            case VirtualHandMode.Snap:
                SnapGrab();
                break;
            case VirtualHandMode.Reparenting:
                ReparentingGrab();
                break;
            case VirtualHandMode.NoReparenting:
                CalculationGrab();
                break;
        }
    }

    #endregion

    #region Custom Methods

    private void SnapGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && canGrab)
            {
                grabbedObject = handCollider.collidingObject;

                // Spawn new Pan if grabbing a Pan
                /*PanSpawner spawner = FindObjectOfType<PanSpawner>();
                if (spawner != null && grabbedObject.CompareTag("Pan"))
                {
                    spawner.SpawnNewPan();
                }
                */
                if (grabbedObject != null && grabbedObject.CompareTag("Pan"))
                {
                    // Find all PanSpawner in the scene
                    PanSpawner[] allSpawners = FindObjectsOfType<PanSpawner>();
                    if (allSpawners.Length == 0)
                    {
                        Debug.LogWarning("No PanSpawner found in scene!");
                    }
                    else
                    {
                        // Pick the closest Spawner to this Pan
                        PanSpawner nearestSpawner = null;
                        float minDist = float.MaxValue;

                        foreach (PanSpawner s in allSpawners)
                        {
                            float dist = Vector3.Distance(s.transform.position, grabbedObject.transform.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearestSpawner = s;
                            }
                        }

                        // Call the closest Spawner to spawn a Pan
                        if (nearestSpawner != null)
                        {
                            Debug.Log($"[{name}] grabbed a Pan. Nearest spawner is {nearestSpawner.name} (distance={minDist}). Spawning new Pan...");
                            nearestSpawner.SpawnNewPan();
                        }
                    }
                }

                if (grabbedObject != null && grabbedObject.CompareTag("Pumpkin"))
                {
                    // Find all PumpkinSpawner in the scene
                    PumpkinSpawner[] allSpawners = FindObjectsOfType<PumpkinSpawner>();
                    if (allSpawners.Length == 0)
                    {
                        Debug.LogWarning("No PumpkinSpawner found in scene!");
                    }
                    else
                    {
                        // Pick the closest Spawner to this Pumpkin
                        PumpkinSpawner nearestSpawner = null;
                        float minDist = float.MaxValue;

                        foreach (PumpkinSpawner s in allSpawners)
                        {
                            float dist = Vector3.Distance(s.transform.position, grabbedObject.transform.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearestSpawner = s;
                            }
                        }

                        // Call the closest Spawner to spawn a Pumpkin
                        if (nearestSpawner != null)
                        {
                            Debug.Log($"[{name}] grabbed a Pumpkin. Nearest spawner is {nearestSpawner.name} (distance={minDist}). Spawning new Pumpkin...");
                            nearestSpawner.SpawnNewPumpkin();
                        }
                    }
                }

                if (grabbedObject != null)
                {
                    if (grabbedObject.CompareTag("Pan"))
                    {
                        // Pan-specific transform offset
                        Vector3 PanOffset = new Vector3(-0.025f, 0, 0);
                        grabbedObject.transform.position = transform.position + transform.rotation * PanOffset;

                        Quaternion PanOffsetRotation = Quaternion.Euler(0, 180, 0);
                        grabbedObject.transform.rotation = transform.rotation * PanOffsetRotation;
                    }
                    else if (grabbedObject.CompareTag("Pumpkin"))
                    {
                        // Pumpkin-specific transform offset
                        Vector3 PumpkinOffset = new Vector3(-0.035f, 0, 0);
                        grabbedObject.transform.position = transform.position + transform.rotation * PumpkinOffset;

                        Quaternion PumpkinOffsetRotation = Quaternion.Euler(0, 180, 0);
                        grabbedObject.transform.rotation = transform.rotation * PumpkinOffsetRotation;
                    }
                    else
                    {
                        // Default transform for other objects
                        grabbedObject.transform.position = transform.position;
                        grabbedObject.transform.rotation = transform.rotation;
                    }
                }
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
                var PanThrow = grabbedObject.GetComponent<PanThrow>();
                if (PanThrow != null)
                {
                    PanThrow.OnReleased(transform);
                }

                grabbedObject.GetComponent<ObjectAccessHandler>().Release();

                // Disable pumpkin shooting script when released
                if (grabbedObject.CompareTag("Pumpkin"))
                {
                    var PumpkinThrow = grabbedObject.GetComponent<PanThrow>();
                    if (PumpkinThrow != null)
                    {
                        PumpkinThrow.OnReleased(transform);
                    }
                }
            }

            grabbedObject = null;
        }
    }

    private void ReparentingGrab()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                grabbedObject.transform.SetParent(transform, true);
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
 
                grabbedObject.GetComponent<ObjectAccessHandler>().Release();
                grabbedObject.transform.SetParent(null, true);
            }

            grabbedObject = null;
        }
    }

    private void CalculationGrab()
    {
        if (grabAction.action.WasPressedThisFrame())
        {
            if (grabbedObject == null && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                offsetMatrix = GetTransformationMatrix(transform, true).inverse *
                               GetTransformationMatrix(grabbedObject.transform, true);
            }
        }
        else if (grabAction.action.IsPressed())
        {
            if (grabbedObject != null)
            {
                Matrix4x4 newTransform = GetTransformationMatrix(transform, true) * offsetMatrix;

                grabbedObject.transform.position = newTransform.GetColumn(3);
                grabbedObject.transform.rotation = newTransform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if(grabbedObject != null)
                grabbedObject.GetComponent<ObjectAccessHandler>().Release();
            grabbedObject = null;
            offsetMatrix = Matrix4x4.identity;
        }
    }

    #endregion
    
    #region Utility Functions

    public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
    {
        if (inWorldSpace)
        {
            return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        else
        {
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }

    #endregion
}

