using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Throwable : MonoBehaviour
{
    private List<Vector3> trackingPos = new List<Vector3>();
    public float velocity = 1000f;
    public int playerID; // Player identification
    private bool pickedUp = false;
    private GameObject parentHand;
    private Rigidbody rb;
    private static Dictionary<int, int> playerThrows = new Dictionary<int, int> { { 1, 0 }, { 2, 0 } };
    private const int maxThrows = 6;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missing Rigidbody on object " + gameObject.name);
        }
    }

    void Update()
    {
        if (pickedUp)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.position = parentHand.transform.position;
            transform.rotation = parentHand.transform.rotation;

            if (trackingPos.Count > 15)
            {
                trackingPos.RemoveAt(0);
            }
            trackingPos.Add(transform.position);

            float triggerRight = Input.GetAxis("Fire1");
            if (triggerRight < 0.1f)
            {
                ThrowObject();
            }
        }
    }

    private void ThrowObject()
    {
        if (!GameManager.Instance.IsGameActive()) return;
        
        if (trackingPos.Count < 2)
        {
            Debug.LogWarning("Not enough data to throw!");
            return;
        }

        if (playerThrows[playerID] >= maxThrows)
        {
            Debug.Log("Player " + playerID + " can no longer throw!");
            return;
        }

        pickedUp = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        Vector3 direction = (trackingPos[trackingPos.Count - 1] - trackingPos[0]).normalized;
        rb.AddForce(direction * velocity);
        GetComponent<Collider>().isTrigger = false;
        trackingPos.Clear();

        playerThrows[playerID]++;
    }

    private void OnTriggerEnter(Collider other)
    {
        float triggerRight = Input.GetAxis("Fire1");

        if (other.CompareTag("hand") && triggerRight > 0.9f)
        {
            PickUpObject(other.gameObject);
        }
    }

    private void PickUpObject(GameObject hand)
    {
        if (playerThrows[playerID] >= maxThrows)
        {
            Debug.Log("Player " + playerID + " can no longer pick up objects!");
            return;
        }
        pickedUp = true;
        parentHand = hand;
        rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
    }
}
