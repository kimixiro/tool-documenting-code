# Unity Documentation System

The Unity Documentation System is a lightweight and convenient documentation system for Unity projects. It provides a custom documentation window within the Unity editor, allowing developers to quickly access and browse through class documentation without leaving the development environment.

## Why Use the Unity Documentation System?

The Unity Documentation System offers a lightweight and integrated solution to access specific information within the Unity editor. It complements traditional documentation systems by providing a simplified and convenient way to quickly find relevant details without disrupting the development workflow.

## Benefits

- **Convenience**: The Unity Documentation System offers a dedicated Documentation window within the Unity editor where developers can search for specific classes, properties, and methods. It provides a concise summary of the relevant information in a lightweight and easily accessible format.

- **Contextual Insights**: The Documentation window not only provides descriptions of classes, properties, and methods but also allows developers to gain insights into the implementation logic. They can find relevant details and helpful comments that provide valuable context without the need to dive deep into the codebase.

## Getting Started

To start using the Unity Documentation System in your Unity project, follow these steps:

1. Import the Unity Documentation System into your project. This can be done by either cloning the repository or downloading the package from the GitHub release.

2. Open the Documentation window within the Unity editor. This window provides a search bar where you can enter your queries.

3. Enter your search query to find relevant documentation. The search system will display matches based on the descriptions provided using the attribute.

4. Browse through the search results and click on the relevant matches to access the detailed information. The documentation will contain concise summaries and comments that offer insights and guidance.

5. Use the Documentation window as a lightweight and integrated notebook to quickly find information and gain contextual understanding of classes, properties, and methods in your project.

## Example Usage

Here's an example demonstrating how the Unity Documentation System enhances your Unity project:

```csharp
[Doc("This is a player character.")]
public class PlayerCharacter : MonoBehaviour
{
    [Doc("The player's health.")]
    public int Health { get; set; }

    [Doc("Moves the player.")]
    public void MovePlayer(Vector3 direction)
    {
        // Movement logic
    }
}
