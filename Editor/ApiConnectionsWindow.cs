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
        private static SettingsProvider CreateSettingsProvider()
        {
            DeucarianEditorProjectSettingsPage page = null;
            return new SettingsProvider("Project/Deucarian/API Connections", SettingsScope.Project)
            {
                label = "API Connections",
                activateHandler = (_, root) =>
                {
                    page?.Dispose();
                    page = new DeucarianEditorProjectSettingsPage(root, "API connections", "Configure project hosts, environments and service bindings in the Control Center.");
                    page.Content.Add(DeucarianEditorWorkspaceControls.Button("Open API connections", Open, true));
                },
                deactivateHandler = () => { page?.Dispose(); page = null; }
            };
        }
    }
}
