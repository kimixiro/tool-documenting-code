using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class DocAttribute : Attribute
{
    public string Description { get; private set; }

    public DocAttribute(string description)
    {
        Description = description;
    }
}