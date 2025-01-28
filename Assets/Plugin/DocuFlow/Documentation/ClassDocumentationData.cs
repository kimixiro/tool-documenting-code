using System;
using System.Collections.Generic;

namespace Plugin.DocuFlow.Documentation
{
    /// <summary>
    /// Хранит данные о документируемом классе: описание, методы, свойства.
    /// </summary>
    public class ClassDocumentationData
    {
        public Type ClassType { get; }
        public string Description { get; }

        private readonly List<MethodDocumentationData> methodsData = new List<MethodDocumentationData>();
        public IReadOnlyList<MethodDocumentationData> MethodsData => methodsData;

        private readonly List<PropertyDocumentationData> propertiesData = new List<PropertyDocumentationData>();
        public IReadOnlyList<PropertyDocumentationData> PropertiesData => propertiesData;

        public ClassDocumentationData(Type classType, string description)
        {
            ClassType = classType;
            Description = description;
        }

        public void AddMethodData(MethodDocumentationData methodData)
        {
            if (methodData == null) throw new ArgumentNullException(nameof(methodData));
            methodsData.Add(methodData);
        }

        public void AddPropertyData(PropertyDocumentationData propertyData)
        {
            if (propertyData == null) throw new ArgumentNullException(nameof(propertyData));
            propertiesData.Add(propertyData);
        }
    }
}