using UnityEngine;

[Doc("This class handles player input.")]
public class PlayerInputHandler
{
    [Doc("This property holds the current input.")]
    public Vector2 CurrentInput { get; private set; }

    [Doc("This method processes the input.")]
    public void ProcessInput() { /*...*/ }
}