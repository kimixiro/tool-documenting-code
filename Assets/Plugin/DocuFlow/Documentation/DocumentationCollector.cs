using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Plugin.DocuFlow.Documentation
{
    /// <summary>
    /// Отвечает за сбор (через рефлексию) всех классов, помеченных [Doc].
    /// </summary>
    public static class DocumentationCollector
    {
        /// <summary>
        /// Асинхронно собирает документацию со всех классов, помеченных [Doc].
        /// </summary>
        public static async Task<List<ClassDocumentationData>> GetDocumentationAsync()
        {
            return await Task.Run(GetDocumentation);
        }

        /// <summary>
        /// Синхронный сбор документации (если нужно).
        /// </summary>
        public static List<ClassDocumentationData> GetDocumentation()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(a => a.GetTypes());

            var result = new List<ClassDocumentationData>();

            foreach (var type in allTypes)
            {
                var classDocAttribute = GetAttribute<DocAttribute>(type);
                if (classDocAttribute == null) 
                    continue;

                var classData = new ClassDocumentationData(type, classDocAttribute.Description);

                // Методы
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var attr = GetAttribute<DocAttribute>(method);
                    if (attr != null)
                    {
                        classData.AddMethodData(new MethodDocumentationData(method, attr.Description));
                    }
                }

                // Свойства
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    var attr = GetAttribute<DocAttribute>(prop);
                    if (attr != null)
                    {
                        classData.AddPropertyData(new PropertyDocumentationData(prop, attr.Description));
                    }
                }

                result.Add(classData);
            }

            return result;
        }

        private static T GetAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttribute<T>();
        }
    }
}
