using UnityEngine;

[Doc("Class representing the ball in the game. Controls the ball's movement and interactions using Unity's physics engine and physics materials.")]
public class Ball : MonoBehaviour
{
    public float launchSpeed = 5.0f;
    public float minSpeed = 4.0f; // Minimum speed to maintain
    private Rigidbody rb;

    [Doc("Gets the Rigidbody component and launches the ball.")]
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Launch();
    }

    [Doc("Ensures the ball maintains a minimum speed.")]
    private void FixedUpdate()
    {
        if (rb.velocity.magnitude < minSpeed)
        {
            rb.velocity = rb.velocity.normalized * minSpeed;
        }
    }

    [Doc("Launches the ball with an initial upward velocity.")]
    public void Launch()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        // Assign an initial speed towards the top of the screen
        rb.velocity = Vector3.up * launchSpeed;
    }

    [Doc("Handles the ball's collision with the paddle and bricks.")]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Calculate new direction based on the paddle's hit point
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 paddleCenter = collision.gameObject.transform.position;
            float paddleWidth = collision.gameObject.transform.localScale.x;
            float relativeIntersect = (hitPoint.x - paddleCenter.x) / paddleWidth;
        
            // Adjust the reflection angle based on where the ball hit the paddle
            float bounceAngle = relativeIntersect * 75f * Mathf.Deg2Rad; // Max bounce angle is 75 degrees
            Vector3 direction = new Vector3(Mathf.Sin(bounceAngle), Mathf.Cos(bounceAngle), 0f);
        
            // Maintain the same speed, change direction
            rb.velocity = direction * rb.velocity.magnitude;
        }
    }

    [Doc("Resets the ball to its original position and stops it from moving.")]
    public void Reset()
    {
        // Stop the ball from moving
        rb.velocity = Vector3.zero;

        // Reset the position of the ball to the center of the screen
        transform.position = Vector3.zero;
    }
}



