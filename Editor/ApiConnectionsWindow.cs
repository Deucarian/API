using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.API.Editor
{
    public sealed class ApiConnectionsWindow : EditorWindow
    {
        private DeucarianEditorPageSession session;
        public static void Open() => DeucarianEditorToolWindow.Open(DeucarianToolIds.ApiConnections);
        public static IDeucarianEditorPage CreatePage() => new ApiConnectionsPage().Page;
        private void CreateGUI()
        {
            session?.Dispose();
            session = new DeucarianEditorPageSession(this, DeucarianToolIds.ApiConnections, CreatePage());
        }
        private void OnDisable() { session?.Dispose(); session = null; }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider() =>
            new SettingsProvider("Project/Deucarian/API Connections", SettingsScope.Project)
            {
                label = "API Connections",
                activateHandler = (_, root) =>
                {
                    var panel = DeucarianEditorInspector.CreateToolkit("API connections");
                    panel.Add(DeucarianEditorWorkspaceControls.Label("Configure project hosts and service bindings.", "dw-muted"));
                    panel.Add(DeucarianEditorWorkspaceControls.Button("Open API connections", Open, true));
                    root.Add(panel);
                }
            };
    }
}
