using UnityEngine;
using UnityEngine.SceneManagement;

[Doc("Singleton class to manage the overall game state.")]
public class GameManager : MonoBehaviour
{
    [Doc("Provides access to the singleton instance of this class.")]
    public static GameManager Instance { get; private set; }

    [Doc("The current score of the game.")]
    public int Score { get; private set; }
    
    public Paddle paddle;
    public Ball ball;
    public Transform brickContainer;
    public GameObject brickPrefab;

    private BrickMapGenerator brickMapGenerator;
    private bool isGameActive = false;
    
    [Doc("Singleton pattern implementation. If another instance exists, it gets destroyed.")]
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            brickMapGenerator = new BrickMapGenerator(brickPrefab, brickContainer);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Doc("Starts the game when this script's object is enabled.")]
    private void Start()
    {
        StartGame();
    }

    [Doc("Starts the game by setting up the score, paddle, ball, and bricks.")]
    public void StartGame()
    {
        Score = 0;
        isGameActive = true;
        paddle.gameObject.SetActive(true);
        ball.gameObject.SetActive(true);
        brickMapGenerator.CreateBricks();
        ball.Launch();
    }


    [Doc("Ends the game by disabling the paddle, ball, and destroying the bricks.")]
    public void EndGame()
    {
        isGameActive = false;
        paddle.gameObject.SetActive(false);
        ball.gameObject.SetActive(false);
        brickMapGenerator.DestroyBricks();
    }

    [Doc("Checks for the user's input to launch the ball.")]
    private void Update()
    {
        if (!isGameActive)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ball.Launch();
        }

        CheckBallOutOfBounds();
    }

    
    [Doc("Checks whether the ball has gone off-screen.")]
    private void CheckBallOutOfBounds()
    {
        if (ball.transform.position.y < paddle.transform.position.y - 1)
        {
            // Reset the ball and the paddle
            ball.Reset();
            paddle.Reset();

            // Optionally, you could also end the game or decrease a life counter
            // EndGame();
            // DecreaseLife();
        }
    }

    [Doc("Increases the score by a given amount.")]
    public void IncrementScore(int points)
    {
        Score += points;
    }
}