using System.Reflection;

public class PropertyDocumentationData
{
    public PropertyInfo Property { get; private set; }
    public string Description { get; private set; }

    public PropertyDocumentationData(PropertyInfo property, string description)
    {
        Property = property;
        Description = description;
    }
}