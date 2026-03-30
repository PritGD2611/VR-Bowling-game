using UnityEngine;

/// <summary>
/// Temporarily add this to one pin to test collision
/// </summary>
public class PinCollisionDebug : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"PIN HIT BY: {collision.gameObject.name}" +
                  $" Force: {collision.impulse.magnitude}");
    }

    void Start()
    {
        // Check own components
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        if (rb == null)
            Debug.LogError($"{gameObject.name}: NO RIGIDBODY!");
        else
            Debug.Log($"{gameObject.name}: Rigidbody OK - Mass:{rb.mass}");

        if (col == null)
            Debug.LogError($"{gameObject.name}: NO COLLIDER!");
        else
            Debug.Log($"{gameObject.name}: Collider OK - {col.GetType().Name}");
    }
}