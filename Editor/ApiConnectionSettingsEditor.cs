using System;
using Deucarian.API.Configuration;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.API.Editor
{
    internal enum ApiServiceDefinitionOwnership { Missing = 0, ProjectOwned = 1, PackageManaged = 2, External = 3 }

    [CustomEditor(typeof(ApiConnectionSettings))]
    internal sealed class ApiConnectionSettingsEditor : UnityEditor.Editor
    {
        private ApiConnectionForm form;
        private IVisualElementScheduledItem refresh;
        public override VisualElement CreateInspectorGUI()
        {
            form?.Dispose();
            refresh?.Pause();
            var root = DeucarianEditorInspector.CreateToolkit("API connection");
            var fields = new VisualElement();
            root.Add(fields);
            form = new ApiConnectionForm(fields, (ApiConnectionSettings)target);
            refresh = root.schedule.Execute(() => form?.Refresh()).Every(500);
            return root;
        }
        private void OnDisable() { refresh?.Pause(); form?.Dispose(); form = null; }

        internal static string GetBaseUrlLabel(ApiNamedClientDefinition client, int clientCount)
        {
            if (clientCount <= 1) return "Base URL";
            string clientId = client?.ClientId?.Trim();
            return string.IsNullOrWhiteSpace(clientId) ? "Client Base URL" : clientId + " Base URL";
        }

        internal static ApiServiceDefinitionOwnership GetDefinitionOwnership(ApiServiceDefinition definition)
        {
            if (definition == null) return ApiServiceDefinitionOwnership.Missing;
            string path = AssetDatabase.GetAssetPath(definition)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path)) return ApiServiceDefinitionOwnership.External;
            if (path.StartsWith("Packages/", StringComparison.Ordinal)) return ApiServiceDefinitionOwnership.PackageManaged;
            return path.StartsWith("Assets/", StringComparison.Ordinal) ? ApiServiceDefinitionOwnership.ProjectOwned : ApiServiceDefinitionOwnership.External;
        }

        internal static bool IsProjectOwned(UnityEngine.Object value) =>
            value != null && (AssetDatabase.GetAssetPath(value)?.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal) ?? false);
    }
}
