# UnityDoc: A Simple Unity Documentation System

## Overview
UnityDoc is a lightweight, custom documentation system for Unity projects. It aims to make it easy for developers to write readable, accessible documentation directly in their code. This system uses C# attributes to annotate code elements, such as classes, properties, and methods, to provide meaningful descriptions of their purpose and functionality.

![Example of documentation](https://drive.google.com/file/d/1bFV3WjjNtkrQIc6KPn8QcAeGdOJ1FdDY/view?usp=sharing)

## Usage
To use UnityDoc, add a `Doc` attribute above any class, property, or method you want to document. 

Here is an example:

```csharp
[Doc("This class represents the player-controlled paddle, handling its movement and reset.")]
public class Paddle : MonoBehaviour
{
    [Doc("The velocity of the paddle.")]
    public Vector3 Velocity { get; private set; }

    [Doc("Resets the paddle's position to the start state.")]
    public void Reset()
    {
        transform.position = resetPosition;
    }
}
```      
In this example, the Paddle class, the Velocity property, and the Reset method are documented with meaningful descriptions.

Please note that the standard Unity methods (like Start, Update, etc.) and private variables are not typically documented using the Doc attribute.

## Recommendations
Write concise, meaningful descriptions for your code elements.
Clearly describe the purpose and functionality of classes and methods.
For methods, describe the arguments, the return type, and side effects if any.
Use consistent language and terminology throughout your documentation.
Regularly update your documentation as your code evolves.
## Conclusion
UnityDoc is a simple, yet effective way to document your Unity codebase. By using this system, you can improve code readability, facilitate knowledge transfer between developers, and make it easier to maintain and extend your projects. Happy coding!
