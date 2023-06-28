using System.Reflection;

public class MethodDocumentationData
{
    public MethodInfo Method { get; private set; }
    public string Description { get; private set; }

    public MethodDocumentationData(MethodInfo method, string description)
    {
        Method = method;
        Description = description;
    }
}