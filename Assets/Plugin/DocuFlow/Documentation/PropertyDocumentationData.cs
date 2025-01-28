using System;
using System.Reflection;

namespace Plugin.DocuFlow.Documentation
{
    public class PropertyDocumentationData
    {
        public PropertyInfo Property { get; }
        public string Description { get; }

        public PropertyDocumentationData(PropertyInfo property, string description)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}