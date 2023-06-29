using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.Callbacks;

public class DocWindow : EditorWindow
{
    private TextField searchField;
    private ScrollView leftScrollView;
    private ScrollView rightScrollView;
    private ListView nameList;
    private Label name;
    private Label description;
    private VisualElement propertiesList;
    private VisualElement methodsList;
    private ClassDocumentationData selectedData;
    
    private Button selectedButton = null;

    private List<ClassDocumentationData>
        filteredDocumentation; // Stores the filtered documentation based on the search query

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
        name = rightScrollView.Q<Label>("class-name");
        description = rightScrollView.Q<Label>("class-description");
        propertiesList = rightScrollView.Q<VisualElement>("properties-list");
        methodsList = rightScrollView.Q<VisualElement>("methods-list");

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
            var classButton = new Button(() => SelectData(classDoc)) {text = classDoc.ClassType.Name};
            classButton.name = classDoc.ClassType.Name; // Give each button a unique name
            classButton.AddToClassList("button"); // Add the class to the button
            nameList.hierarchy.Add(classButton);
        }
        
        if (filteredDocumentation.Count > 0)
        {
            SelectData(filteredDocumentation[0]);
        }
    }

    private void RefreshDocumentation()
    {
        // Clear panes
        name.text = "";
        description.text = "";
        propertiesList.Clear();
        methodsList.Clear();

        if (selectedData != null)
        {
            name.text = selectedData.ClassType.Name;
            description.text = selectedData.Description;

            foreach (var propertyData in selectedData.PropertiesData)
            {
                var propertyLabel = new Label(propertyData.Property.Name);
                propertyLabel.AddToClassList("header");
                var propertyDescription = new Label(propertyData.Description);
                propertyDescription.AddToClassList("description");

                propertiesList.Add(propertyLabel);
                propertiesList.Add(propertyDescription);
            }

            foreach (var methodData in selectedData.MethodsData)
            {
                var methodLabel = new Label(methodData.Method.Name);
                methodLabel.AddToClassList("header");
                var methodDescription = new Label(methodData.Description);
                methodDescription.AddToClassList("description");

                methodsList.Add(methodLabel);
                methodsList.Add(methodDescription);
            }
        }
    }
    
    private void SelectData(ClassDocumentationData data)
    {
        // Unhighlight the previously selected button
        if (selectedButton != null)
        {
            selectedButton.RemoveFromClassList("selected");
        }

        // Find the new selected button
        selectedButton = nameList.Q<Button>(data.ClassType.Name);
        // Highlight the new selected button
        selectedButton.AddToClassList("selected");

        selectedData = data;
        RefreshDocumentation();
    }

    private void FilterNameList(string query)
    {
        // Convert the search query to lowercase for case-insensitive comparison
        string lowercaseQuery = query.ToLower();

        // Filter the documentation based on the search query
        filteredDocumentation = DocumentationManager.Documentation.Where(classDoc =>
        {
            string allText = $"{classDoc.ClassType.Name} {classDoc.Description} " +
                             $"{string.Join(" ", classDoc.MethodsData.Select(m => $"{m.Method.Name} {m.Description}"))} " +
                             $"{string.Join(" ", classDoc.PropertiesData.Select(p => $"{p.Property.Name} {p.Description}"))}";

            return allText.ToLower().Contains(lowercaseQuery);
        }).ToList();

        // Clear and repopulate the name list
        nameList.hierarchy.Clear();
        foreach (var classDoc in filteredDocumentation)
        {
            var classButton = new Button(() => SelectData(classDoc)) {text = classDoc.ClassType.Name};
            classButton.AddToClassList("button"); // Add the class to the button
            nameList.hierarchy.Add(classButton);
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
