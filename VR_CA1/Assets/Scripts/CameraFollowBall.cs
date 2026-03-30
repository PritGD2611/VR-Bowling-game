using UnityEngine;

/// <summary>
/// Attach to your Main Camera or XR Origin.
/// Makes camera follow the ball while it's rolling.
/// </summary>
public class CameraFollowBall : MonoBehaviour
{
    [SerializeField] BowlingBall targetBall;
    [SerializeField] float followSpeed = 5f;
    [SerializeField] bool followX = true;
    [SerializeField] bool followZ = true;
    [SerializeField] bool followY = false; // Keep camera height same

    Vector3 originalPosition;
    bool isFollowing = false;

    void Start()
    {
        originalPosition = transform.position;

        if (targetBall != null)
        {
            targetBall.OnBallReleased += StartFollowing;
            targetBall.OnBallStopped += StopFollowing;
        }
    }

    void StartFollowing(BowlingBall ball)
    {
        isFollowing = true;
    }

    void StopFollowing(BowlingBall ball)
    {
        isFollowing = false;
        // Return camera to original position
        transform.position = originalPosition;
    }

    void LateUpdate()
    {
        if (!isFollowing || targetBall == null) return;

        Vector3 targetPos = transform.position;

        if (followX) targetPos.x = targetBall.transform.position.x;
        if (followZ) targetPos.z = targetBall.transform.position.z;
        if (followY) targetPos.y = targetBall.transform.position.y;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }
}