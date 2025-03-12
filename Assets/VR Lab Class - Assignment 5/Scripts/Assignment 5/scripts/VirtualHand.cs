using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


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
    if (grabAction.action.IsPressed())  // Keep checking while button is held
    {
        if (grabbedObject == null && canGrab)
        {
            grabbedObject = handCollider.collidingObject;

            if (grabbedObject != null && grabbedObject.CompareTag("Pan"))
            {
                PanSpawner[] allSpawners = FindObjectsOfType<PanSpawner>();
                if (allSpawners.Length > 0)
                {
                    PanSpawner nearestSpawner = allSpawners.OrderBy(s => Vector3.Distance(s.transform.position, grabbedObject.transform.position)).FirstOrDefault();
                    nearestSpawner?.SpawnNewPan();
                }
            }

            if (grabbedObject != null && grabbedObject.CompareTag("Pumpkin"))
            {
                PumpkinSpawner[] allSpawners = FindObjectsOfType<PumpkinSpawner>();
                if (allSpawners.Length > 0)
                {
                    PumpkinSpawner nearestSpawner = allSpawners.OrderBy(s => Vector3.Distance(s.transform.position, grabbedObject.transform.position)).FirstOrDefault();
                    nearestSpawner?.SpawnNewPumpkin();
                }
            }
        }

        if (grabbedObject != null) // Keep updating position while button is held
        {
            if (grabbedObject.CompareTag("Pan"))
            {
                Vector3 PanOffset = new Vector3(-0.025f, 0, 0);
                grabbedObject.transform.position = transform.position + transform.rotation * PanOffset;
                grabbedObject.transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            else if (grabbedObject.CompareTag("Pumpkin"))
            {
                Vector3 PumpkinOffset = new Vector3(-0.035f, 0, 0);
                grabbedObject.transform.position = transform.position + transform.rotation * PumpkinOffset;
                grabbedObject.transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            else
            {
                grabbedObject.transform.position = transform.position;
                grabbedObject.transform.rotation = transform.rotation;
            }
        }
    }
    else if (grabAction.action.WasReleasedThisFrame()) // Release when button is released
    {
        if (grabbedObject != null)
        {
            var panThrow = grabbedObject.GetComponent<PanThrow>();
            if (panThrow != null)
            {
                panThrow.OnReleased(transform);
            }

            grabbedObject.GetComponent<ObjectAccessHandler>().Release();

            if (grabbedObject.CompareTag("Pumpkin"))
            {
                var pumpkinThrow = grabbedObject.GetComponent<PanThrow>();
                if (pumpkinThrow != null)
                {
                    pumpkinThrow.OnReleased(transform);
                }
            }

            grabbedObject = null; // Reset grabbed object
        }
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

