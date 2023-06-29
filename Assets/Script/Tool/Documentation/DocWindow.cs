using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Debug = UnityEngine.Debug;

public class DocWindow : EditorWindow
{
    private TextField searchField;
    private ScrollView leftScrollView;
    private ScrollView rightScrollView;
    private ListView nameList;
    private Label classNameLabel;
    private Label description;
    private VisualElement propertiesList;
    private VisualElement methodsList;
    private ClassDocumentationData selectedData;

    private Label selectedLabel = null;

    private List<ClassDocumentationData>
        filteredDocumentation; // Stores the filtered documentation based on the search query

    private Stack<ClassDocumentationData> history = new Stack<ClassDocumentationData>(); // Add this line

    [MenuItem("Window/DocWindow")]
    public static void ShowWindow()
    {
        GetWindow<DocWindow>("Documentation");
    }

    public void OnEnable()
    {
        SetupUIElements();
        PopulateNameList();
        RefreshDocumentation();

        Button backButton = rootVisualElement.Q<Button>("back-button");
        if (backButton != null)
        {
            backButton.clicked += NavigateBack;
        }
    }

    private void SetupUIElements()
    {
        // Load the UXML file
        var visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Script/Tool/Documentation/UI/DocumentationWindow.uxml");
        if (visualTree == null)
        {
            Debug.LogError("UXML file not found. Make sure the path is correct.");
            return;
        }

        // Clone the UXML tree to the root visual element
        visualTree.CloneTree(rootVisualElement);

        // Load the USS file
        var styleSheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Script/Tool/Documentation/UI/WindowStyle.uss");
        if (styleSheet == null)
        {
            Debug.LogError("USS file not found. Make sure the path is correct.");
            return;
        }

        // Apply the style sheet to the root visual element
        rootVisualElement.styleSheets.Add(styleSheet);

        // Get the UI elements
        searchField = rootVisualElement.Q<TextField>("search-query");
        leftScrollView = rootVisualElement.Q<ScrollView>("left-scroll-view");
        rightScrollView = rootVisualElement.Q<ScrollView>("right-scroll-view");
        nameList = leftScrollView.Q<ListView>("name-list");
        classNameLabel = rightScrollView.Q<Label>("class-name");
        description = rightScrollView.Q<Label>("class-description");
        propertiesList = rightScrollView.Q<VisualElement>("properties-list");
        methodsList = rightScrollView.Q<VisualElement>("methods-list");

        leftScrollView.style.flexBasis = 300;

        rightScrollView.style.flexDirection = FlexDirection.Row;
        rightScrollView.style.flexGrow = 1;


        // Refresh documentation when search field changes
        searchField.RegisterValueChangedCallback(evt => FilterNameList(evt.newValue));
    }

    private void PopulateNameList()
    {
        if (DocumentationManager.Documentation == null)
        {
            DocumentationManager.RefreshDocumentation();
        }

        // Initialize filtered documentation with all the classes
        filteredDocumentation = DocumentationManager.Documentation.ToList();

        foreach (var classDoc in filteredDocumentation)
        {
            var classNameLabel = new Label(classDoc.ClassType.Name);
            classNameLabel.name = classDoc.ClassType.Name; // Give each label a unique name
            classNameLabel.AddToClassList("class-name-label"); // Add the class to the label
            classNameLabel.AddManipulator(new Clickable(() => SelectData(classDoc))); // Make the label clickable
            nameList.hierarchy.Add(classNameLabel);
        }

        if (filteredDocumentation.Count > 0)
        {
            SelectData(filteredDocumentation[0]);
        }
    }

    private void RefreshDocumentation()
    {
        // Clear panes
        classNameLabel.text = "";
        description.text = "";
        propertiesList.Clear();
        methodsList.Clear();

        if (selectedData != null)
        {
            classNameLabel.text = selectedData.ClassType.Name;
            description.text = selectedData.Description;

            foreach (var propertyData in selectedData.PropertiesData)
            {
                var propertyLabel = new Label(propertyData.Property.Name);
                propertyLabel.AddToClassList("header");
                propertyLabel.AddManipulator(new Clickable(() =>
                    OpenScriptAtLine(propertyData.Property))); // Add this line
                var propertyDescription = new Label(propertyData.Description);
                propertyDescription.AddToClassList("description");

                propertiesList.Add(propertyLabel);
                propertiesList.Add(propertyDescription);
            }

            foreach (var methodData in selectedData.MethodsData)
            {
                var methodLabel = new Label(methodData.Method.Name);
                methodLabel.AddToClassList("header");
                methodLabel.AddManipulator(new Clickable(() => OpenScriptAtLine(methodData.Method))); // Add this line
                var methodDescription = new Label(methodData.Description);
                methodDescription.AddToClassList("description");

                methodsList.Add(methodLabel);
                methodsList.Add(methodDescription);
            }
        }
    }

    private void OpenScriptAtLine(MemberInfo member)
    {
        var allScriptAssets = AssetDatabase.FindAssets("t:script");
        string memberClassName = member.DeclaringType.Name;

        TextAsset targetAsset = null;
        foreach (var scriptAssetGUID in allScriptAssets)
        {
            string scriptAssetPath = AssetDatabase.GUIDToAssetPath(scriptAssetGUID);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptAssetPath);

            if (fileNameWithoutExtension == memberClassName)
            {
                targetAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptAssetPath);
                break;
            }
        }

        if (targetAsset == null)
        {
            Debug.LogError($"Failed to find script file for class {memberClassName}");
            return;
        }

        AssetDatabase.OpenAsset(targetAsset);
    }


    private void SelectData(ClassDocumentationData data)
    {
        // Unhighlight the previously selected label
        if (selectedLabel != null)
        {
            selectedLabel.RemoveFromClassList("selected");
        }

        // Find the label corresponding to the selected data in the name list
        selectedLabel = nameList.Q<Label>(data.ClassType.Name);

        // If the label was found, highlight it
        if (selectedLabel != null)
        {
            selectedLabel.AddToClassList("selected");
        }

        // Push the previously selected data to the history stack
        if (selectedData != null)
        {
            history.Push(selectedData);
        }

        // Set the selected data
        selectedData = data;

        // Refresh the documentation
        RefreshDocumentation();
    }


    private void FilterNameList(string query)
    {
        // Convert the search query to lowercase for case-insensitive comparison
        string lowercaseQuery = query.ToLower();

        // Filter and score the documentation based on the search query
        var scoredDocumentation = DocumentationManager.Documentation.Select(classDoc =>
            {
                int score = 0;
                if (classDoc.ClassType.Name.ToLower().Contains(lowercaseQuery)) score += 1000;
                if (classDoc.MethodsData.Any(m => m.Method.Name.ToLower().Contains(lowercaseQuery))) score += 500;
                if (classDoc.PropertiesData.Any(p => p.Property.Name.ToLower().Contains(lowercaseQuery))) score += 500;
                if (classDoc.Description.ToLower().Contains(lowercaseQuery)) score += 1;

                return new {Doc = classDoc, Score = score};
            })
            .Where(scoredDoc => scoredDoc.Score > 0) // Only include documents that match the query
            .OrderByDescending(scoredDoc => scoredDoc.Score) // Sort by score
            .Select(scoredDoc => scoredDoc.Doc) // Select the documentation
            .ToList();

        // Assign the filtered and scored documentation
        filteredDocumentation = scoredDocumentation;

        // Clear and repopulate the name list
        nameList.hierarchy.Clear();
        foreach (var classDoc in filteredDocumentation)
        {
            var classNameLabel = new Label(classDoc.ClassType.Name);
            classNameLabel.AddToClassList("class-name-label"); // Add the class to the label
            classNameLabel.AddManipulator(new Clickable(() => SelectData(classDoc))); // Make the label clickable
            nameList.hierarchy.Add(classNameLabel);
        }

        // Unhighlight the previously selected label
        if (selectedLabel != null)
        {
            selectedLabel.RemoveFromClassList("selected");
        }

        if (filteredDocumentation.Contains(selectedData))
        {
            // Find the label corresponding to the selected data in the name list
            selectedLabel = nameList.Q<Label>(selectedData.ClassType.Name);

            // If the label was found, highlight it
            if (selectedLabel != null)
            {
                selectedLabel.AddToClassList("selected");
            }
        }
        else if (filteredDocumentation.Count > 0)
        {
            // If the selected data isn't in the filtered documentation, select the first data
            SelectData(filteredDocumentation[0]);

            // Highlight the first label
            if (nameList.Children().Any())
            {
                selectedLabel = nameList.Children().First() as Label;
                if (selectedLabel != null)
                {
                    selectedLabel.AddToClassList("selected");
                }
            }
        }
        else
        {
            // If there is no filtered documentation, clear the selected data and label
            selectedData = null;
            selectedLabel = null;
        }
    }


    private void NavigateBack()
    {
        if (history.Count > 0)
        {
            ClassDocumentationData lastViewed = history.Pop();
            SelectData(lastViewed);
        }
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // Refresh the documentation.
        DocumentationManager.RefreshDocumentation();

        // Find any open DocWindows and refresh them.
        foreach (DocWindow window in Resources.FindObjectsOfTypeAll<DocWindow>())
        {
            window.RefreshDocumentation();
        }
    }
}