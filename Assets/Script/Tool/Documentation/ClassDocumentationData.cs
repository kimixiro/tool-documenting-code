using System;
using System.Collections.Generic;

public class ClassDocumentationData
{
    public Type ClassType { get; }
    public string Description { get; }

    public List<MethodDocumentationData> MethodsData { get; }
    public List<PropertyDocumentationData> PropertiesData { get; }

    public ClassDocumentationData(Type classType, string description)
    {
        ClassType = classType;
        Description = description;
        MethodsData = new List<MethodDocumentationData>();
        PropertiesData = new List<PropertyDocumentationData>();
    }
}

