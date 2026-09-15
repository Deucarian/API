using System;
using System.Collections.Generic;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine.UIElements;
using Ui = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.API.Editor
{
    internal sealed class ApiConnectionForm : IDisposable
    {
        private readonly VisualElement root;
        private readonly ApiConnectionSettings settings;
        private readonly List<DeucarianEditorSerializedForm> bindings = new List<DeucarianEditorSerializedForm>();
        private readonly List<DeucarianEditorWorkspaceForm> live = new List<DeucarianEditorWorkspaceForm>();
        private int environmentIndex;
        private string message;
        private Label feedback;

        internal ApiConnectionForm(VisualElement root, ApiConnectionSettings settings)
        {
            this.root = root;
            root.AddToClassList("dw-service-form");
            this.settings = settings;
            Render();
        }

        internal void Refresh()
        {
            if (settings == null) return;
            foreach (var form in live) form.Refresh();
        }

        private void Render()
        {
            ClearBindings(); live.Clear(); root.Clear();
            if (settings == null) return;
            var definition = settings.ServiceDefinition;
            var form = Form(root);
            if (definition == null) { root.Add(Ui.Label("A service definition is required.", "dw-muted")); return; }
            if (!definition.TryGetEnvironmentDescriptors(out IReadOnlyList<ApiEnvironmentDescriptor> descriptors, out string error))
            { root.Add(Ui.Label(error, "dw-muted")); return; }
            if (descriptors.Count == 0) { root.Add(Ui.Label("This service has no environments.", "dw-muted")); return; }
            var names = new List<string>();
            foreach (var descriptor in descriptors) names.Add(descriptor.DisplayName);
            environmentIndex = Math.Min(environmentIndex, names.Count - 1);
            form.Choice("api-environment", "Environment", names, () => environmentIndex,
                value => { environmentIndex = value; message = null; Render(); });
            root.Add(Ui.Divider());
            form.ReadOnly("api-service-id", "Service", () => definition.ServiceId);
            var selected = descriptors[environmentIndex];
            var environment = Find(settings.Environments, selected.EnvironmentId);
            if (environment == null)
            {
                root.Add(Ui.Label("This environment needs a project connection slot.", "dw-muted"));
                var add = Ui.Button("Add missing connection slots", Synchronize, true);
                add.SetEnabled(ApiConnectionSettingsEditor.IsProjectOwned(settings)); root.Add(add);
            }
            else
            {
                bool editable = ApiConnectionSettingsEditor.IsProjectOwned(settings) && ApiConnectionSettingsEditor.IsProjectOwned(environment);
                var fields = new VisualElement(); root.Add(fields);
                var hosts = Form(fields);
                for (int i = 0; i < environment.Clients.Count; i++)
                {
                    var client = environment.Clients[i];
                    if (client == null) { fields.Add(Ui.Label("A named client is missing.", "dw-muted")); continue; }
                    var host = hosts.Text("api-host-" + i, ApiConnectionSettingsEditor.GetBaseUrlLabel(client, environment.Clients.Count),
                        () => client.BaseUrl ?? string.Empty, value =>
                        {
                            Undo.RecordObject(environment, "Configure API host");
                            client.BaseUrl = value;
                            EditorUtility.SetDirty(environment);
                            Refresh();
                        });
                    host.tooltip = "Enter the real " + selected.DisplayName + " service address. Example only: " +
                        HostExample(selected.Stage) + ". No address is selected automatically.";
                }
                hosts.Note(() => "Configure the " + selected.DisplayName + " host explicitly. Other environments keep their own addresses.");
                fields.SetEnabled(editable);
                form.ReadOnly("api-configuration", "Status", () => environment == null ? "Missing" : environment.ClassifyConfiguration(out _).ToString());
                var note = form.ReadOnly("api-configuration-details", "Details", () =>
                {
                    if (environment == null) return "Missing environment";
                    var state = environment.ClassifyConfiguration(out string reason);
                    return state == ApiEnvironmentProfileConfigurationState.Configured ? "Hosts are valid. Network availability has not been tested."
                        : state == ApiEnvironmentProfileConfigurationState.NotConfigured ? "Requests stay blocked until hosts are configured." : reason;
                });
                note.AddToClassList("dw-muted");
                var save = Ui.Button("Save", Save); save.SetEnabled(editable);
                root.Add(Ui.EndActions(Ui.IconButton("Check configuration", DeucarianEditorIconIds.Check, Check, DeucarianEditorButtonRole.Primary), save));
                root.Add(Ui.Divider());
                if (!editable) root.Add(Ui.Label("Package-managed settings are read-only. Create a project connection to make changes.", "dw-muted"));
                BuildAdvanced(definition, environment, editable, note.parent);
            }
            feedback = Ui.Label(message, "dw-muted"); root.Add(feedback);
            Ui.Show(feedback, !string.IsNullOrEmpty(message));
        }

        private void BuildAdvanced(ApiServiceDefinition definition, ApiEnvironmentProfile environment, bool editable, VisualElement configurationDetails)
        {
            var advanced = new Foldout { text = "Advanced settings", value = false };
            advanced.AddToClassList("dw-foldout"); root.Add(advanced);
            advanced.Add(configurationDetails);
            var form = Form(advanced);
            var asset = form.Asset("api-definition", "Service definition", typeof(ApiServiceDefinition), () => definition, _ => { });
            asset.SetEnabled(false);
            form.ReadOnly(null, "Contract owner", () => ApiConnectionSettingsEditor.GetDefinitionOwnership(definition).ToString());
            form.ReadOnly(null, "Source version", () => string.IsNullOrEmpty(definition.SourceVersion) ? "Not provided" : definition.SourceVersion);
            var policies = new VisualElement(); advanced.Add(policies);
            policies.Add(Ui.Label("Request policies and non-secret headers. Never store credentials here.", "dw-muted"));
            var binding = new DeucarianEditorSerializedForm(policies, environment); bindings.Add(binding);
            binding.Property("defaultRequestPolicy", "Environment policy");
            for (int i = 0; i < environment.Clients.Count; i++)
            {
                if (environment.Clients[i] == null) continue;
                binding.Property("clients.Array.data[" + i + "].requestPolicy", environment.Clients[i].ClientId + " policy");
                binding.Property("clients.Array.data[" + i + "].defaultHeaders", environment.Clients[i].ClientId + " headers");
            }
            policies.SetEnabled(editable);
            var sync = Ui.Button("Synchronize connection slots", Synchronize);
            sync.SetEnabled(ApiConnectionSettingsEditor.IsProjectOwned(settings)); advanced.Add(sync);
        }

        private void Check()
        {
            message = settings.TryValidate(out string error) ? "Connection structure is valid. No network request was made." : error;
            ShowMessage(); Refresh();
        }

        private void Save()
        {
            foreach (var environment in settings.Environments)
                if (environment != null && ApiConnectionSettingsEditor.IsProjectOwned(environment)) AssetDatabase.SaveAssetIfDirty(environment);
            AssetDatabase.SaveAssetIfDirty(settings);
            message = "Connection settings saved."; ShowMessage();
        }

        private void Synchronize()
        {
            message = ApiConnectionSettingsAssetFactory.TrySynchronizeProjectSettings(settings, out int added, out string error)
                ? added == 0 ? "Connection slots are synchronized." : "Added " + added + " blank connection slots." : error;
            Render();
        }

        private void ShowMessage() { if (feedback != null) { feedback.text = message; Ui.Show(feedback, true); } }
        private DeucarianEditorWorkspaceForm Form(VisualElement parent)
        { var form = new DeucarianEditorWorkspaceForm(parent); live.Add(form); return form; }
        private void ClearBindings() { foreach (var binding in bindings) binding.Dispose(); bindings.Clear(); }
        public void Dispose() { ClearBindings(); live.Clear(); }

        private static ApiEnvironmentProfile Find(IReadOnlyList<ApiEnvironmentProfile> environments, ApiEnvironmentId id)
        {
            foreach (var environment in environments)
                if (environment != null && environment.TryGetId(out var candidate) && candidate == id) return environment;
            return null;
        }

        internal static string HostExample(ApiEnvironmentStage stage) =>
            "https://" + (Enum.IsDefined(typeof(ApiEnvironmentStage), stage) ? stage.ToString().ToLowerInvariant() : "custom") + ".example.invalid";
    }
}
