using UnityEngine;

/// <summary>
/// Add this to your CatchFloor object.
/// When ball falls and hits this floor, it auto resets.
/// </summary>
public class ResetOnContact : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Check if the thing that hit us is a bowling ball
        BowlingBall ball = collision.gameObject.GetComponent<BowlingBall>();
        if (ball != null)
        {
            Debug.Log("Ball hit catch floor - Resetting!");
            ball.ResetBall();
        }
    }
}