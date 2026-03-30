using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class BowlingBall : MonoBehaviour
{
    [Header("Pin Target")]
    [SerializeField] public Transform pinTarget;

    [Header("Roll Settings")]
    [SerializeField] float rollSpeed = 800f;

    [Header("Stop Forcing Velocity When This Close to Pins")]
    [SerializeField] float stopForcingDistance = 5f;

    [Header("Tags")]
    [SerializeField] string[] validGroundTags = { "Ground", "Floor", "Lane" };
    [SerializeField] string[] validPinTags = { "Pin" };
    [SerializeField] string[] validWallTags = { "BackWall", "Wall" };
    [SerializeField]
    string[] ignoreTags =
        { "Ball", "BallStand", "Untagged", "Player" };

    [Header("Reset Settings")]
    [SerializeField] float resetHeightThreshold = -50f;
    [SerializeField] float stopCheckDelay = 4f;
    [SerializeField] float stopSpeedThreshold = 1f;

    Rigidbody rb;
    XRGrabInteractable grabInteractable;

    Vector3 originalPosition;
    Quaternion originalRotation;

    bool isGrabbed = false;
    bool hasLanded = false;
    bool isRolling = false;
    bool hasScored = false;
    bool invalidThrow = false;
    bool hitAnyPin = false;
    // KEY: Once close to pins, stop forcing velocity
    bool reachedPinZone = false;

    float timeSinceRolling = 0f;

    public System.Action<BowlingBall> OnBallReleased;
    public System.Action<BowlingBall> OnBallStopped;
    public System.Action<BowlingBall> OnBallMissed;
    public System.Action<BowlingBall> OnInvalidThrow;
    public System.Action<BowlingBall> OnBallDestroyed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.movementType =
            XRBaseInteractable.MovementType.Kinematic;
        grabInteractable.throwOnDetach = false;
        grabInteractable.retainTransformParent = false;

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (pinTarget == null)
        {
            GameObject pt = GameObject.Find("PinTarget");
            if (pt != null)
                pinTarget = pt.transform;
            else
                Debug.LogError("NO PinTarget found!");
        }
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        hasLanded = false;
        isRolling = false;
        invalidThrow = false;
        hasScored = false;
        hitAnyPin = false;
        reachedPinZone = false;
        timeSinceRolling = 0f;
        CancelInvoke();

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"GRABBED: {gameObject.name}");
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (args.isCanceled) return;

        isGrabbed = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"RELEASED: {gameObject.name}");
        OnBallReleased?.Invoke(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isGrabbed) return;

        string hitTag = collision.gameObject.tag;
        string hitName = collision.gameObject.name;

        Debug.Log($"Hit: {hitName} | Tag: {hitTag}");

        // ── Ground ────────────────────────────────────
        if (IsGroundTag(hitTag) && !hasLanded)
        {
            hasLanded = true;
            isRolling = true;
            reachedPinZone = false;
            timeSinceRolling = 0f;
            Debug.Log("✅ GROUND - Rolling!");
            StartRolling();
            return;
        }

        if (IsGroundTag(hitTag) && hasLanded) return;

        // ── Pin ───────────────────────────────────────
        if (IsPinTag(hitTag))
        {
            hitAnyPin = true;
            reachedPinZone = true; // Stop forcing velocity
            isRolling = false;// Let physics handle pin collision
            Debug.Log($"✅ PIN HIT: {hitName}");

            // Start countdown to score
            if (!hasScored)
            {
                hasScored = true;
                Invoke(nameof(FireBallStopped), 2f);
            }
            return;
        }

        // ── BackWall ──────────────────────────────────
        if (IsWallTag(hitTag) && !hasScored)
        {
            hasScored = true;
            isRolling = false;

            if (!hitAnyPin)
            {
                Debug.Log("❌ BackWall - NO pins hit = MISS");
                Invoke(nameof(FireBallMissed), 0.5f);
            }
            else
            {
                Debug.Log("✅ BackWall after pins = Score");
                Invoke(nameof(FireBallStopped), 0.5f);
            }
            return;
        }

        // ── Ignore ────────────────────────────────────
        if (IsIgnoreTag(hitTag)) return;

        // ── Invalid before landing ────────────────────
        if (!hasLanded && !invalidThrow)
            Invoke(nameof(CheckIfStillNotLanded), 0.5f);
    }

    void CheckIfStillNotLanded()
    {
        if (isGrabbed || hasLanded) return;
        if (!invalidThrow)
        {
            invalidThrow = true;
            FireInvalidThrow();
        }
    }

    // ═══════════════════════════════════════════════
    // ROLLING
    // ═══════════════════════════════════════════════
    void StartRolling()
    {
        if (pinTarget == null)
        {
            Debug.LogError("pinTarget NULL!");
            return;
        }

        Vector3 dir = GetDirectionToPins();

        Debug.Log($"ROLLING → Dir:{dir} Speed:{rollSpeed}");

        rb.linearVelocity = new Vector3(
            dir.x * rollSpeed,
            0f,
            dir.z * rollSpeed
        );
    }

    Vector3 GetDirectionToPins()
    {
        if (pinTarget == null) return Vector3.forward;

        return new Vector3(
            pinTarget.position.x - transform.position.x,
            0f,
            pinTarget.position.z - transform.position.z
        ).normalized;
    }

    float GetDistanceToPins()
    {
        if (pinTarget == null) return 999f;

        return Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(pinTarget.position.x, pinTarget.position.z)
        );
    }

    void FixedUpdate()
    {
        if (isGrabbed) return;

        if (isRolling && pinTarget != null)
        {
            timeSinceRolling += Time.fixedDeltaTime;

            float distToPins = GetDistanceToPins();

            // ── KEY FIX ──────────────────────────────
            // When ball is close to pins, STOP forcing velocity
            // Let physics naturally carry ball into pins
            if (distToPins <= stopForcingDistance)
            {
                if (!reachedPinZone)
                {
                    reachedPinZone = true;
                    Debug.Log($"Pin zone reached! Dist:{distToPins:F1}" +
                              " - Stopping force, letting physics handle");
                    // Don't modify velocity - let ball coast naturally
                }
                // No velocity forcing here - ball rolls freely into pins
            }
            else
            {
                // Far from pins - keep forcing velocity toward pins
                Vector3 dir = GetDirectionToPins();
                rb.linearVelocity = new Vector3(
                    dir.x * rollSpeed,
                    rb.linearVelocity.y,
                    dir.z * rollSpeed
                );
            }

            // Ball stopped naturally
            if (timeSinceRolling > stopCheckDelay && !hasScored)
            {
                float speed = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.z).magnitude;

                if (speed < stopSpeedThreshold)
                {
                    isRolling = false;
                    hasScored = true;

                    if (!hitAnyPin)
                    {
                        Debug.Log("Stopped - no pins hit = MISS");
                        FireBallMissed();
                    }
                    else
                    {
                        Debug.Log("Stopped after pins = Score");
                        FireBallStopped();
                    }
                }
            }

            // Fell off
            if (transform.position.y < resetHeightThreshold)
                FireBallDestroyed();
        }
    }

    // ═══════════════════════════════════════════════
    // TAG HELPERS
    // ═══════════════════════════════════════════════
    bool IsGroundTag(string tag)
    {
        foreach (string t in validGroundTags)
            if (tag == t) return true;
        return false;
    }

    bool IsPinTag(string tag)
    {
        foreach (string t in validPinTags)
            if (tag == t) return true;
        return false;
    }

    bool IsWallTag(string tag)
    {
        foreach (string t in validWallTags)
            if (tag == t) return true;
        return false;
    }

    bool IsIgnoreTag(string tag)
    {
        foreach (string t in ignoreTags)
            if (tag == t) return true;
        return false;
    }

    // ═══════════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════════
    void FireBallStopped()
    {
        CancelInvoke(nameof(FireBallStopped));
        isRolling = false;
        Debug.Log("Ball stopped - counting score!");
        OnBallStopped?.Invoke(this);
    }

    void FireBallMissed()
    {
        CancelInvoke(nameof(FireBallMissed));
        isRolling = false;
        Debug.Log("MISS!");
        OnBallMissed?.Invoke(this);
    }

    void FireInvalidThrow()
    {
        CancelInvoke(nameof(FireInvalidThrow));
        isRolling = false;
        Debug.Log("GUTTER - 0pts");
        OnInvalidThrow?.Invoke(this);
        ResetBall();
    }

    void FireBallDestroyed()
    {
        CancelInvoke();
        if (invalidThrow) return;
        invalidThrow = true;
        isRolling = false;
        Debug.Log("Out of bounds");
        OnBallDestroyed?.Invoke(this);
        ResetBall();
    }

    // ═══════════════════════════════════════════════
    // RESET
    // ═══════════════════════════════════════════════
    public void ResetBall()
    {
        CancelInvoke();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        rb.isKinematic = false;

        isGrabbed = false;
        hasLanded = false;
        isRolling = false;
        invalidThrow = false;
        hasScored = false;
        hitAnyPin = false;
        reachedPinZone = false;
        timeSinceRolling = 0f;

        Debug.Log($"RESET: {gameObject.name} → {originalPosition}");
    }
}