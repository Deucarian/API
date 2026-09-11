using System;
using System.Collections.Generic;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Ui = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.API.Editor
{
    internal sealed class ApiConnectionsPage
    {
        private readonly DeucarianEditorCollectionWorkspace view;
        private ApiConnectionForm connection;
        private ApiServiceDefinition definition;
        private ApiConnectionSettings draft;
        private string selectedGuid, selectedService, message;
        private bool adding;
        private bool bindingsChanged;
        public IDeucarianEditorPage Page { get; }

        internal ApiConnectionsPage()
        {
            var root = new VisualElement();
            view = new DeucarianEditorCollectionWorkspace(root, Application.productName, "API connections",
                "Configure service connections and per-environment base URLs.", DeucarianToolIds.ApiConnections, "Find a service…");
            view.UsePanels();
            view.Collection.AddToClassList("dw-service-collection");
            Ui.Show(view.Workspace.Scope, false);
            Ui.Show(view.Workspace.Tabs, false);
            view.Workspace.SearchField.RegisterValueChangedCallback(_ => RenderList());
            var list = view.Collection.Q<ScrollView>("workspace-collection");
            list.Add(Ui.Divider());
            list.Add(Ui.Actions(Ui.Button("+ Add connection", () =>
            { adding = true; selectedGuid = selectedService = null; message = null; Render(); })));
            ApiConnectionProjectSettings.instance.BindingsChanged += OnBindingsChanged;
            Page = new DeucarianEditorPage(root, activate: _ => Update(), update: _ => Update(), dispose: Dispose);
            Render();
        }

        private void Render()
        {
            bindingsChanged = false;
            RenderList(); RenderDetails();
        }

        private void OnBindingsChanged() => bindingsChanged = true;

        private void Update()
        {
            if (bindingsChanged)
            {
                bool selectedExists = false;
                foreach (var binding in ApiConnectionProjectSettings.instance.Bindings)
                    if (binding != null && binding.ServiceId == selectedService && binding.SettingsGuid == selectedGuid)
                        selectedExists = true;
                if (!selectedExists) selectedGuid = selectedService = null;
                Render();
            }
            else connection?.Refresh();
        }

        private void RenderList()
        {
            var items = new List<DeucarianEditorCollectionItem>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in ApiConnectionProjectSettings.instance.Bindings)
            {
                if (binding == null) continue;
                if ((binding.ServiceId ?? "").IndexOf(view.Workspace.SearchField.value ?? "", StringComparison.OrdinalIgnoreCase) < 0) continue;
                var settings = AssetDatabase.LoadAssetAtPath<ApiConnectionSettings>(AssetDatabase.GUIDToAssetPath(binding.SettingsGuid));
                if (selectedGuid == null && !adding) { selectedGuid = binding.SettingsGuid; selectedService = binding.ServiceId; }
                string id = binding.ServiceId + "|" + binding.SettingsGuid;
                if (!seen.Add(id)) continue;
                items.Add(new DeucarianEditorCollectionItem(id, binding.ServiceId, string.Empty, settings == null ? "Missing asset" : string.Empty,
                    () => { selectedGuid = binding.SettingsGuid; selectedService = binding.ServiceId; adding = false; message = null; Render(); }, iconId: "database"));
            }
            view.SetItems(items, selectedService + "|" + selectedGuid, "No bound connections. Add a service to get started.");
        }

        private void RenderDetails()
        {
            connection?.Dispose(); connection = null;
            var root = view.Details;
            root.Clear();
            if (adding || selectedGuid == null)
            {
                RenderAdd(root);
                return;
            }
            var settings = AssetDatabase.LoadAssetAtPath<ApiConnectionSettings>(AssetDatabase.GUIDToAssetPath(selectedGuid));
            if (settings == null) root.Add(Ui.Label("The bound connection asset is missing. Remove this binding or choose a replacement.", "dw-muted"));
            else
            {
                var fields = new VisualElement(); root.Add(fields);
                connection = new ApiConnectionForm(fields, settings);
            }
            var binding = new Foldout { text = "Project binding", value = false };
            binding.AddToClassList("dw-foldout"); root.Add(binding);
            var asset = new DeucarianEditorWorkspaceForm(binding).Asset("api-bound-settings", "Connection settings", typeof(ApiConnectionSettings),
                () => settings, value => { draft = value as ApiConnectionSettings; BindDraft(); });
            asset.tooltip = "Choose a replacement project-owned connection for this service.";
            binding.Add(Ui.Button("Remove binding", () =>
            {
                if (!ApiServiceId.TryParse(selectedService, out var id)) return;
                if (!EditorUtility.DisplayDialog("Remove connection binding", "Unbind " + selectedService + "? Its asset will be kept.", "Remove binding", "Cancel")) return;
                ApiConnectionProjectSettings.instance.Clear(id);
                selectedGuid = selectedService = null; Render();
            }, DeucarianEditorButtonRole.Destructive));
            if (!string.IsNullOrEmpty(message)) root.Add(Ui.Label(message, "dw-muted"));
        }

        private void RenderAdd(VisualElement root)
        {
            root.Add(Ui.Label("Add a connection", "dw-section-title"));
            root.Add(Ui.Label("Choose a service definition, then create project settings or bind an existing asset.", "dw-muted"));
            var form = new DeucarianEditorWorkspaceForm(root);
            form.Asset("api-add-definition", "Service definition", typeof(ApiServiceDefinition), () => definition,
                value => { definition = value as ApiServiceDefinition; RenderDetails(); });
            form.Asset("api-add-settings", "Existing settings", typeof(ApiConnectionSettings), () => draft,
                value => { draft = value as ApiConnectionSettings; RenderDetails(); });
            var create = Ui.Button("Create settings", Create, true); create.SetEnabled(definition != null);
            var bind = Ui.Button("Bind existing", BindDraft); bind.SetEnabled(draft != null);
            root.Add(Ui.EndActions(bind, create));
            if (!string.IsNullOrEmpty(message)) root.Add(Ui.Label(message, "dw-muted"));
        }

        private void Create()
        {
            if (definition == null) return;
            string path = EditorUtility.SaveFilePanelInProject("Create API connection", definition.name + "ConnectionSettings", "asset", "Choose a project-owned asset path.");
            if (string.IsNullOrWhiteSpace(path)) return;
            if (ApiConnectionSettingsAssetFactory.TryCreateProjectSettings(path, definition, out draft, out message)) BindDraft();
            else RenderDetails();
        }

        private void BindDraft()
        {
            if (!adding && selectedService != null && draft != null && draft.ServiceDefinition?.ServiceId != selectedService)
            { message = "The replacement must describe the same service."; RenderDetails(); return; }
            if (ApiConnectionProjectSettings.instance.TryBind(draft, out message))
            {
                selectedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(draft));
                selectedService = draft.ServiceDefinition.ServiceId;
                adding = false; message = "Connection bound to this project.";
            }
            Render();
        }

        private void Dispose()
        {
            ApiConnectionProjectSettings.instance.BindingsChanged -= OnBindingsChanged;
            connection?.Dispose(); view.Dispose();
        }
    }
}
