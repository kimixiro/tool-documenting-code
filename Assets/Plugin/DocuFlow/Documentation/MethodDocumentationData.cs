using System;
using System.Reflection;

namespace Plugin.DocuFlow.Documentation
{
    public class MethodDocumentationData
    {
        public MethodInfo Method { get; }
        public string Description { get; }

        public MethodDocumentationData(MethodInfo method, string description)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}