using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Manages the collection of class documentation data within the application.
/// </summary>
public static class DocumentationManager
{
    /// <summary>
    /// Gets the list of class documentation data.
    /// </summary>
    public static List<ClassDocumentationData> Documentation { get; private set; }
    
    /// <summary>
    /// Refreshes the list of class documentation data by gathering the latest data from all documented classes.
    /// </summary>
    public static void RefreshDocumentation()
    {
        Documentation = DocumentationCollector.GetDocumentation();
    }
}
