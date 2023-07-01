using UnityEngine;

[Doc("Class representing a brick in the game. Can be destroyed by the ball.")]
public class Brick : MonoBehaviour
{
    [Doc("Destroys the brick when the ball collides with it.")]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            DestroyBrick();
        }
    }

    [Doc("Destroys the brick and increments the player's score.")]
    public void DestroyBrick()
    {
        GameManager.Instance.IncrementScore(1);
        Destroy(gameObject);
    }
}
