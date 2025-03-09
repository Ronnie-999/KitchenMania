// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;

// public class MouseBehaviour : NetworkBehaviour
// {
//     [HideInInspector] public List<Transform> movementTargets;
//     private int currentTargetIdx = -1;
//     private float movementSpeed = 2f;
//     private float maxSpeed = 5f;
//     private float speedIncreaseRate = 0.1f;

//     private void Start()
//     {
//         if (IsServer)
//             UpdateTarget();
//     }

//     private void Update()
//     {
//         if (IsServer)
//             MoveMouse();
//     }

//     private void UpdateTarget()
//     {
//         int newIdx = currentTargetIdx;
//         while (newIdx == currentTargetIdx)
//             newIdx = Random.Range(0, movementTargets.Count);
//         currentTargetIdx = newIdx;
//     }

//     private void MoveMouse()
//     {
//         if (movementTargets.Count == 0) return;

//         float distance = Vector3.Distance(transform.position, movementTargets[currentTargetIdx].position);
//         if (distance > 0.5f)
//         {
//             Vector3 direction = (movementTargets[currentTargetIdx].position - transform.position).normalized;
//             transform.position += direction * (movementSpeed * Time.deltaTime);
//             transform.rotation = Quaternion.LookRotation(direction);
//             movementSpeed = Mathf.Min(movementSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
//         }
//         else
//         {
//             UpdateTarget();
//         }
//     }
// }



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseBehaviour : NetworkBehaviour
{
    public List<Transform> movementTargets;
    private int currentTargetIdx = 0;
    public float movementSpeed = 3f;
    public float jumpForce = 5f;
    public float destroyDelay = 1.0f;
    private Rigidbody rb;

    public static System.Action<int> OnMouseHit; // Event do przekazywania punktacji

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Brak Rigidbody na obiekcie myszy!");
        }
    }

    private void Update()
    {
        MoveMouse();
    }

    private void MoveMouse()
    {
        if (movementTargets.Count == 0) return;

        Transform target = movementTargets[currentTargetIdx];
        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * (movementSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentTargetIdx = (currentTargetIdx + 1) % movementTargets.Count;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Throwable"))
        {
            Throwable throwable = collision.gameObject.GetComponent<Throwable>();
            if (throwable != null)
            {
                OnMouseHit?.Invoke(throwable.playerID); // Przypisanie punktu graczowi
            }
            HitReaction();
        }
    }

    private void HitReaction()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
