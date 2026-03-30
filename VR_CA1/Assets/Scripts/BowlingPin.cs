using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to each pre-placed bowling pin in the scene.
/// Automatically remembers where you placed it.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BowlingPin : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Angle from upright at which pin is considered fallen")]
    [SerializeField] float fallenAngleThreshold = 45f;

    Rigidbody rb;
    Vector3 originalPosition;
    Quaternion originalRotation;
    bool isFallen = false;
    bool hasBeenCounted = false;

    public bool IsFallen => isFallen;
    public bool HasBeenCounted
    {
        get => hasBeenCounted;
        set => hasBeenCounted = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Automatically save where YOU placed this pin in the scene
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        float angle = Vector3.Angle(Vector3.up, transform.up);
        isFallen = angle > fallenAngleThreshold;
    }

    public bool CheckIfFallen()
    {
        float angle = Vector3.Angle(Vector3.up, transform.up);
        isFallen = angle > fallenAngleThreshold;
        return isFallen;
    }

    /// <summary>
    /// Reset pin back to where it was originally placed in the scene.
    /// </summary>
    public void ResetPin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        isFallen = false;
        hasBeenCounted = false;

        StartCoroutine(BriefKinematicReset());
    }

    IEnumerator BriefKinematicReset()
    {
        rb.isKinematic = true;
        yield return new WaitForFixedUpdate();
        rb.isKinematic = false;
    }

    // Keep this method but it's optional now since Awake handles it
    public void SetOriginalPosition(Vector3 pos, Quaternion rot)
    {
        originalPosition = pos;
        originalRotation = rot;
    }
}