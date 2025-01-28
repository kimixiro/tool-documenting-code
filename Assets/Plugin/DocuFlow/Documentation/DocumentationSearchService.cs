using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.DocuFlow.Documentation
{
    /// <summary>
    /// Сервис для поиска по документации: простое вхождение + Левенштейн.
    /// Если вам нужен префиксный поиск, можно добавить Trie здесь же.
    /// </summary>
    public static class DocumentationSearchService
    {
        private const int MaxLevenshteinDistance = 2;

        /// <summary>
        /// Выполняет поиск по списку документации, фильтруя по query.
        /// </summary>
        public static List<ClassDocumentationData> FilterDocumentation(string query, List<ClassDocumentationData> documentation)
        {
            if (string.IsNullOrWhiteSpace(query))
                return documentation;

            var queryLower = query.ToLowerInvariant();

            var containsFiltered = documentation
                .Where(doc => IsMatchByContains(doc, queryLower))
                .ToList();

            var fuzzyFiltered = documentation
                .Where(doc => IsMatchByLevenshtein(doc, queryLower))
                .ToList();

            return containsFiltered.Union(fuzzyFiltered).Distinct().ToList();
        }

        private static bool IsMatchByContains(ClassDocumentationData doc, string queryLower)
        {
            // Class name + description
            if (doc.ClassType.FullName.ToLowerInvariant().Contains(queryLower))
                return true;

            if (doc.Description.ToLowerInvariant().Contains(queryLower))
                return true;

            // Methods
            if (doc.MethodsData.Any(m =>
                m.Method.Name.ToLowerInvariant().Contains(queryLower) ||
                m.Description.ToLowerInvariant().Contains(queryLower)))
            {
                return true;
            }

            // Properties
            if (doc.PropertiesData.Any(p =>
                p.Property.Name.ToLowerInvariant().Contains(queryLower) ||
                p.Description.ToLowerInvariant().Contains(queryLower)))
            {
                return true;
            }

            return false;
        }

        private static bool IsMatchByLevenshtein(ClassDocumentationData doc, string queryLower)
        {
            if (ComputeLevenshteinDistance(doc.ClassType.FullName.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance)
                return true;

            if (ComputeLevenshteinDistance(doc.Description.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance)
                return true;

            if (doc.MethodsData.Any(m =>
                ComputeLevenshteinDistance(m.Method.Name.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance ||
                ComputeLevenshteinDistance(m.Description.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance))
            {
                return true;
            }

            if (doc.PropertiesData.Any(p =>
                ComputeLevenshteinDistance(p.Property.Name.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance ||
                ComputeLevenshteinDistance(p.Description.ToLowerInvariant(), queryLower) <= MaxLevenshteinDistance))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Расстояние Левенштейна.
        /// </summary>
        private static int ComputeLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) 
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target)) 
                return source.Length;

            var matrix = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= target.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[source.Length, target.Length];
        }
    }
}
