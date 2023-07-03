using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.DocuFlow.Documentation
{
    public static class SearchManager
    {
        private static readonly TrieNode root = new TrieNode();

        // Build Trie tree for the given list of class documentation
        public static void BuildTrie(List<ClassDocumentationData> documentation)
        {
            foreach (var classDoc in documentation)
            {
                Insert(classDoc.ClassType.Name);
                foreach (var method in classDoc.MethodsData)
                {
                    Insert(method.Method.Name);
                }
                foreach (var property in classDoc.PropertiesData)
                {
                    Insert(property.Property.Name);
                }
                Insert(classDoc.Description);
            }
        }

        private static void Insert(string key)
        {
            var node = root;
            foreach (var ch in key.ToLowerInvariant())
            {
                if (!node.Children.ContainsKey(ch))
                {
                    node.Children[ch] = new TrieNode();
                }
                node = node.Children[ch];
            }
            node.IsEndOfWord = true;
        }

        // Trie based filter
        public static List<ClassDocumentationData> FilterAndScoreDocumentation(string query, List<ClassDocumentationData> documentation)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return documentation;
            }

            var filteredDocs = new List<ClassDocumentationData>();
            if (Search(query))
            {
                filteredDocs = documentation
                    .Where(classDoc => classDoc.ClassType.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                       classDoc.MethodsData.Any(m => m.Method.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                       classDoc.PropertiesData.Any(p => p.Property.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                       classDoc.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            return filteredDocs;
        }

        // Search a query string in the Trie tree
        private static bool Search(string key)
        {
            var node = root;
            foreach (var ch in key.ToLowerInvariant())
            {
                if (!node.Children.ContainsKey(ch))
                {
                    return false;
                }
                node = node.Children[ch];
            }
            return node != null && node.IsEndOfWord;
        }
    }
}


