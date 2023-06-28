using System.Collections.Generic;
using UnityEditor;
public static class DocumentationManager
{
    public static List<ClassDocumentationData> Documentation { get; private set; }
    
    public static void RefreshDocumentation()
    {
        Documentation = DocumentationCollector.GetDocumentation();
    }
}
