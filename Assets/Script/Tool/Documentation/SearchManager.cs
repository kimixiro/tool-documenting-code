using System;
using System.Collections.Generic;
using System.Linq;

public static class SearchManager
{
    public static List<ClassDocumentationData> FilterAndScoreDocumentation(string query, List<ClassDocumentationData> documentation)
    {
        return documentation
            .Select(classDoc => new { Doc = classDoc, Score = ScoreDocumentation(classDoc, query) })
            .Where(scoredDoc => scoredDoc.Score > 0)
            .OrderByDescending(scoredDoc => scoredDoc.Score)
            .Select(scoredDoc => scoredDoc.Doc)
            .ToList();
    }

    private static int ScoreDocumentation(ClassDocumentationData classDoc, string query)
    {
        int score = 0;
        score += GetScoreFromClassType(classDoc.ClassType.Name, query);
        score += GetScoreFromMethods(classDoc.MethodsData, query);
        score += GetScoreFromProperties(classDoc.PropertiesData, query);
        score += GetScoreFromDescription(classDoc.Description, query);
        return score;
    }

    private static int GetScoreFromClassType(string typeName, string query)
    {
        return typeName.Contains(query, StringComparison.OrdinalIgnoreCase) ? 1000 : 0;
    }

    private static int GetScoreFromMethods(IReadOnlyList<MethodDocumentationData> methods, string query)
    {
        return methods.Any(m => m.Method.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ? 500 : 0;
    }

    private static int GetScoreFromProperties(IReadOnlyList<PropertyDocumentationData> properties, string query)
    {
        return properties.Any(p => p.Property.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ? 500 : 0;
    }

    private static int GetScoreFromDescription(string description, string query)
    {
        return description.Contains(query, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }
}

