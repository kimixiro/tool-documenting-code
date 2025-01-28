using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;

namespace Plugin.DocuFlow.Documentation
{
    /// <summary>
    /// Хранит текущую коллекцию документации (список ClassDocumentationData).
    /// Обновляет её при перезагрузке скриптов или по запросу.
    /// </summary>
    public static class DocumentationManager
    {
        public static List<ClassDocumentationData> Documentation { get; private set; }

        /// <summary>
        /// Асинхронное обновление документации.
        /// </summary>
        public static async Task RefreshDocumentationAsync()
        {
            Documentation = await DocumentationCollector.GetDocumentationAsync();
        }

        /// <summary>
        /// Синхронный вариант (если надо).
        /// </summary>
        public static void RefreshDocumentation()
        {
            Documentation = DocumentationCollector.GetDocumentation();
        }
    }

    /// <summary>
    /// Автоматическая пересборка документации после перезагрузки скриптов.
    /// Можно убрать или изменить логику, если хотите управлять вручную.
    /// </summary>
    [InitializeOnLoad]
    public static class DocumentationAutoRefresher
    {
        static DocumentationAutoRefresher()
        {
            // Вызывается при перезагрузке сборок в Editor
            EditorApplication.delayCall += async () =>
            {
                await DocumentationManager.RefreshDocumentationAsync();
            };
        }
    }
}