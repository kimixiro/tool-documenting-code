using System;
using UnityEngine;

[Doc("Class representing the player's paddle. Controls the paddle's movement.")]
public class Paddle : MonoBehaviour
{
    public float movementSpeed = 10f;
    private Vector3 lastPosition;
    public Vector3 Velocity { get; private set; }
    
    public Vector3 resetPosition = Vector3.zero;

    // Define your movement boundaries (e.g., the left and right sides of the screen)
    public float leftBoundary = -8.0f;
    public float rightBoundary = 8.0f;

    private void Start()
    {
        resetPosition = transform.position;
    }

    [Doc("Captures player's input and translates the paddle accordingly.")]
    private void Update()
    {
        // Record the paddle's position at the start of the frame
        lastPosition = transform.position;
        
        float horizontalInput = Input.GetAxis("Horizontal");
        float movement = horizontalInput * movementSpeed * Time.deltaTime;
        transform.Translate(movement, 0f, 0f);

        // Clamp the paddle's x position to be within the boundaries
        float clampedX = Mathf.Clamp(transform.position.x, leftBoundary, rightBoundary);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // Calculate the paddle's velocity based on its movement this frame
        Velocity = (transform.position - lastPosition) / Time.deltaTime;
    }
    
    [Doc("Resets the paddle to its original position.")]
    public void Reset()
    {
        // Reset the position of the paddle to the center of the screen
        transform.position = resetPosition;
    }
}


