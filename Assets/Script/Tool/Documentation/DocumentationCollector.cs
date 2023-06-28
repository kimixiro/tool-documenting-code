using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class DocumentationCollector
{
    public static List<ClassDocumentationData> GetDocumentation()
    {
        var result = new List<ClassDocumentationData>();
        var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

        foreach (var type in allTypes)
        {
            var classDocAttribute = type.GetCustomAttribute<DocAttribute>();
            if (classDocAttribute == null) continue;

            var classData = new ClassDocumentationData(type, classDocAttribute.Description);
            result.Add(classData);

            var methodsData = type.GetMethods()
                .Select(m => new {method = m, attr = m.GetCustomAttribute<DocAttribute>()})
                .Where(x => x.attr != null);

            foreach(var mData in methodsData)
            {
                classData.MethodsData.Add(new MethodDocumentationData(mData.method, mData.attr.Description));
            }

            var propertiesData = type.GetProperties()
                .Select(p => new {property = p, attr = p.GetCustomAttribute<DocAttribute>()})
                .Where(x => x.attr != null);

            foreach(var pData in propertiesData)
            {
                classData.PropertiesData.Add(new PropertyDocumentationData(pData.property, pData.attr.Description));
            }
        }

        return result;
    }
}
