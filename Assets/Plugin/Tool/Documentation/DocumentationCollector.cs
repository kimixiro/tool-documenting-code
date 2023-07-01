using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class DocumentationCollector
{
    /// <summary>
    /// Gets documentation data from all classes marked with a DocAttribute.
    /// </summary>
    /// <returns>A list of documentation data for each class marked with a DocAttribute.</returns>
    public static List<ClassDocumentationData> GetDocumentation()
    {
        var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
        var result = new List<ClassDocumentationData>();

        foreach (var type in allTypes)
        {
            var classDocAttribute = GetAttribute<DocAttribute>(type);
            if (classDocAttribute == null) continue;

            var classData = new ClassDocumentationData(type, classDocAttribute.Description);

            foreach (var methodData in GetMethodDocumentationData(type))
            {
                classData.AddMethodData(methodData);
            }

            foreach (var propertyData in GetPropertyDocumentationData(type))
            {
                classData.AddPropertyData(propertyData);
            }

            result.Add(classData);
        }

        return result;
    }

    /// <summary>
    /// Gets the documentation data of all methods in a type.
    /// </summary>
    /// <param name="type">The type from which to extract method documentation data.</param>
    /// <returns>A list of method documentation data.</returns>
    private static List<MethodDocumentationData> GetMethodDocumentationData(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(m => new {Method = m, Attribute = GetAttribute<DocAttribute>(m)})
            .Where(x => x.Attribute != null)
            .Select(x => new MethodDocumentationData(x.Method, x.Attribute.Description))
            .ToList();
    }

    /// <summary>
    /// Gets the documentation data of all properties in a type.
    /// </summary>
    /// <param name="type">The type from which to extract property documentation data.</param>
    /// <returns>A list of property documentation data.</returns>
    private static List<PropertyDocumentationData> GetPropertyDocumentationData(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(p => new {Property = p, Attribute = GetAttribute<DocAttribute>(p)})
            .Where(x => x.Attribute != null)
            .Select(x => new PropertyDocumentationData(x.Property, x.Attribute.Description))
            .ToList();
    }

    /// <summary>
    /// Gets a specific attribute from a member.
    /// </summary>
    /// <typeparam name="T">The type of attribute to extract.</typeparam>
    /// <param name="member">The member from which to extract the attribute.</param>
    /// <returns>The extracted attribute if exists; otherwise, null.</returns>
    private static T GetAttribute<T>(MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttribute<T>();
    }
}