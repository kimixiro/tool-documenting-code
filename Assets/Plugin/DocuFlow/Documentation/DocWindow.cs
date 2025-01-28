using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.DocuFlow.Documentation
{
    public class DocWindow : EditorWindow
    {
        // UI элементы
        private TextField searchField;
        private ScrollView leftScrollView;
        private ScrollView rightScrollView;
        private Label classNameLabel;
        private Label description;
        private Label fieldsHeading;
        private Label methodsHeading;
        private VisualElement propertiesList;
        private VisualElement methodsList;
        
        // Мы убираем ListView ("name-list") – будем добавлять элементы прямо внутрь leftScrollView
        // private ListView nameList; // Не нужен, так как будем вручную строить Foldout

        // Данные
        private ClassDocumentationData selectedData;
        private List<ClassDocumentationData> filteredDocumentation;
        
        // Кеш для быстрого доступа к "Label" (или любому VisualElement), чтобы подсветить выбранный
        private Dictionary<string, VisualElement> classElements = new Dictionary<string, VisualElement>();
        private VisualElement selectedElement;

        private Stack<ClassDocumentationData> history = new Stack<ClassDocumentationData>();

        [MenuItem("Window/Documentation")]
        public static void ShowWindow()
        {
            GetWindow<DocWindow>("Documentation");
        }

        private async void OnEnable()
        {
            SetupUI();
            RegisterCallbacks();
            await EnsureDocumentationIsReady();
            RefreshAndDisplayAll();
        }

        /// <summary>
        /// Убеждаемся, что данные в DocumentationManager уже собраны.
        /// Если нет, запускаем сбор.
        /// </summary>
        private async Task EnsureDocumentationIsReady()
        {
            if (DocumentationManager.Documentation == null || DocumentationManager.Documentation.Count == 0)
            {
                await DocumentationManager.RefreshDocumentationAsync();
            }
        }

        private void SetupUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugin/DocuFlow/Documentation/UI/DocumentationWindow.uxml");
            if (visualTree != null)
            {
                visualTree.CloneTree(rootVisualElement);
            }
            else
            {
                Debug.LogError("UXML file not found. Please check the path.");
            }

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugin/DocuFlow/Documentation/UI/WindowStyle.uss");
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError("USS file not found. Please check the path.");
            }

            // Находим нужные элементы
            searchField = rootVisualElement.Q<TextField>("search-query");
            leftScrollView = rootVisualElement.Q<ScrollView>("left-scroll-view");
            rightScrollView = rootVisualElement.Q<ScrollView>("right-scroll-view");
            classNameLabel = rightScrollView.Q<Label>("class-name");
            description = rightScrollView.Q<Label>("class-description");
            fieldsHeading = rightScrollView.Q<Label>("fields-heading");
            methodsHeading = rightScrollView.Q<Label>("methods-heading");
            propertiesList = rightScrollView.Q<VisualElement>("properties-list");
            methodsList = rightScrollView.Q<VisualElement>("methods-list");

            leftScrollView.style.flexBasis = 300;
            rightScrollView.style.flexGrow = 1;
        }

        private void RegisterCallbacks()
        {
            // Поле поиска
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(evt =>
                {
                    FilterDocumentation(evt.newValue);
                });
            }

            // Кнопка "Back"
            var backButton = rootVisualElement.Q<Button>("back-button");
            if (backButton != null)
            {
                backButton.clicked += NavigateBack;
            }
        }

        private void RefreshAndDisplayAll()
        {
            if (DocumentationManager.Documentation == null)
            {
                Debug.LogError("Documentation is null - please ensure it's refreshed.");
                return;
            }

            // Показать все классы без фильтра
            filteredDocumentation = DocumentationManager.Documentation.ToList();
            BuildNamespaceGroupedList(filteredDocumentation);

            if (filteredDocumentation.Any())
            {
                SelectData(filteredDocumentation[0]);
            }
            else
            {
                ClearDetails();
            }
        }

        /// <summary>
        /// Группирует классы по namespace и создаёт Foldout для каждого namespace.
        /// </summary>
        private void BuildNamespaceGroupedList(List<ClassDocumentationData> classDocs)
        {
            // Очищаем левую панель и кеш
            leftScrollView.contentContainer.Clear();
            classElements.Clear();

            // Группируем по namespace
            // Если type.Namespace == null, берём пустую строку
            var groups = classDocs
                .GroupBy(doc => doc.ClassType.Namespace ?? "")
                .OrderBy(g => g.Key); // Сортируем по названию namespace

            foreach (var group in groups)
            {
                // Создаём Foldout для данного namespace
                var foldout = new Foldout
                {
                    text = string.IsNullOrEmpty(group.Key) ? "(Global Namespace)" : group.Key,
                    value = false // изначально свернут или развернут
                };

                // Проходимся по классам в этом namespace
                foreach (var doc in group.OrderBy(d => d.ClassType.Name))
                {
                    foldout.Add(CreateClassLabel(doc));
                }

                leftScrollView.contentContainer.Add(foldout);
            }
        }

        private VisualElement CreateClassLabel(ClassDocumentationData doc)
        {
            // Можно показывать короткое имя, а в tooltip класть FullName
            var shortName = doc.ClassType.Name;
            var fullName = doc.ClassType.FullName;

            var label = new Label(shortName)
            {
                tooltip = fullName
            };
            label.AddToClassList("class-name-label");

            // При клике выбираем класс
            label.AddManipulator(new Clickable(() => SelectData(doc)));

            // Сохраняем в словарь
            classElements[fullName] = label;
            return label;
        }

        private void SelectData(ClassDocumentationData doc)
        {
            // Снимаем выделение с предыдущего элемента
            if (selectedElement != null)
            {
                selectedElement.RemoveFromClassList("selected");
            }

            // Запоминаем в историю (если нужно)
            if (selectedData != null && selectedData != doc)
            {
                history.Push(selectedData);
            }

            // Обновляем выбранный класс
            selectedData = doc;

            // Подсвечиваем новый элемент, если он есть
            if (doc != null && classElements.TryGetValue(doc.ClassType.FullName, out var visualElem))
            {
                selectedElement = visualElem;
                selectedElement.AddToClassList("selected");
            }
            else
            {
                selectedElement = null;
            }

            // Заполняем правую панель
            DisplayClassData(doc);
        }

        private void DisplayClassData(ClassDocumentationData doc)
        {
            ClearDetails();
            if (doc == null) return;

            classNameLabel.text = doc.ClassType.FullName;
            description.text = doc.Description;

            // Свойства
            if (doc.PropertiesData.Any())
            {
                ShowElement(fieldsHeading);
                ShowElement(propertiesList);
                foreach (var prop in doc.PropertiesData)
                {
                    AddPropertyUI(prop);
                }
            }
            else
            {
                HideElement(fieldsHeading);
                HideElement(propertiesList);
            }

            // Методы
            if (doc.MethodsData.Any())
            {
                ShowElement(methodsHeading);
                ShowElement(methodsList);
                foreach (var method in doc.MethodsData)
                {
                    AddMethodUI(method);
                }
            }
            else
            {
                HideElement(methodsHeading);
                HideElement(methodsList);
            }
        }

        private void AddPropertyUI(PropertyDocumentationData propData)
        {
            var propLabel = CreateLabel(propData.Property.Name, "header");
            propLabel.AddManipulator(new Clickable(() => OpenScript(propData.Property)));

            var propDesc = CreateLabel(propData.Description, "description");
            
            propertiesList.Add(propLabel);
            propertiesList.Add(propDesc);
        }

        private void AddMethodUI(MethodDocumentationData methodData)
        {
            var methodLabel = CreateLabel(methodData.Method.Name, "header");
            methodLabel.AddManipulator(new Clickable(() => OpenScript(methodData.Method)));

            var methodDesc = CreateLabel(methodData.Description, "description");
            
            methodsList.Add(methodLabel);
            methodsList.Add(methodDesc);
        }

        private Label CreateLabel(string text, string styleClass)
        {
            var lbl = new Label(text);
            lbl.AddToClassList(styleClass);
            return lbl;
        }

        private void OpenScript(MemberInfo member)
        {
            var allScripts = AssetDatabase.FindAssets("t:script");
            var className = member.DeclaringType.Name;

            TextAsset targetAsset = allScripts
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetFileNameWithoutExtension(path) == className)
                .Select(path => AssetDatabase.LoadAssetAtPath<TextAsset>(path))
                .FirstOrDefault();

            if (targetAsset == null)
            {
                Debug.LogError($"Could not find a matching script file for class {className}");
                return;
            }

            AssetDatabase.OpenAsset(targetAsset);
        }

        private void FilterDocumentation(string query)
        {
            if (DocumentationManager.Documentation == null) return;

            filteredDocumentation = DocumentationSearchService.FilterDocumentation(query, DocumentationManager.Documentation);

            BuildNamespaceGroupedList(filteredDocumentation);

            // Проверяем, остался ли предыдущий выбранный в новом наборе
            if (selectedData != null && !filteredDocumentation.Contains(selectedData))
            {
                // Если нет, выбираем первый, если есть
                if (filteredDocumentation.Any())
                {
                    SelectData(filteredDocumentation[0]);
                }
                else
                {
                    selectedData = null;
                    selectedElement = null;
                    ClearDetails();
                }
            }
            else
            {
                // Обновляем подсветку, если выбранный класс остался
                if (selectedData != null && classElements.TryGetValue(selectedData.ClassType.FullName, out var elem))
                {
                    elem.AddToClassList("selected");
                    selectedElement = elem;
                }
            }
        }

        private void ClearDetails()
        {
            classNameLabel.text = string.Empty;
            description.text = string.Empty;
            propertiesList.Clear();
            methodsList.Clear();
        }

        private void HideElement(VisualElement element)
        {
            element.style.display = DisplayStyle.None;
        }

        private void ShowElement(VisualElement element)
        {
            element.style.display = DisplayStyle.Flex;
        }

        private void NavigateBack()
        {
            if (history.Count > 0)
            {
                SelectData(history.Pop());
            }
        }
    }
}
