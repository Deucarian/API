using System;
using System.Linq;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.API.Editor
{
    internal static class ApiServiceDefinitionAuthoring
    {
        internal static ApiServiceDefinition FindOnlyAvailable()
        {
            var definitions = new DeucarianEditorAssetCatalog(typeof(ApiServiceDefinition)).Find()
                .OfType<ApiServiceDefinition>().Where(value => value.IsValid(out _)).Take(2).ToArray();
            return definitions.Length == 1 ? definitions[0] : null;
        }

        internal static UnityEngine.Object Customize(UnityEngine.Object asset)
        {
            if (!(asset is ApiServiceDefinition source)) return null;
            string path = EditorUtility.SaveFilePanelInProject("Customize service", source.name + " Custom", "asset",
                "Create a project-owned service definition and endpoint catalog.");
            return string.IsNullOrEmpty(path) ? null : Copy(source, path);
        }

        internal static ApiServiceDefinition Copy(ApiServiceDefinition source, string path)
        {
            if (source == null || !DeucarianEditorAssetCatalog.IsUnusedProjectPath(path))
                throw new ArgumentException("Choose a service definition and a new project .asset path.");
            var copy = UnityEngine.Object.Instantiate(source);
            var catalog = source.EndpointCatalog != null ? UnityEngine.Object.Instantiate(source.EndpointCatalog) : null;
            bool saved = false;
            try
            {
                copy.name = System.IO.Path.GetFileNameWithoutExtension(path);
                copy.EndpointCatalog = catalog;
                AssetDatabase.CreateAsset(copy, path); saved = true;
                if (catalog != null) AssetDatabase.AddObjectToAsset(catalog, copy);
                EditorUtility.SetDirty(copy);
                AssetDatabase.SaveAssetIfDirty(copy);
                return copy;
            }
            catch
            {
                if (saved) AssetDatabase.DeleteAsset(path);
                if (copy != null && !AssetDatabase.Contains(copy)) Undo.DestroyObjectImmediate(copy);
                if (catalog != null && !AssetDatabase.Contains(catalog)) Undo.DestroyObjectImmediate(catalog);
                throw;
            }
        }

        internal static void BuildForm(VisualElement root, Action<ApiServiceDefinition> created)
        {
            var form = new DeucarianEditorWorkspaceForm(root).Section("Create custom service");
            string id = "", name = "", route = "";
            HttpMethod method = HttpMethod.GET;
            form.Text("api-new-service-id", "Stable service ID", () => id, value => id = value);
            form.Text("api-new-service-name", "Display name", () => name, value => name = value);
            form.Text("api-new-service-route", "First endpoint path", () => route, value => route = value);
            form.Enum("api-new-service-method", "Method", () => method, value => method = value);
            form.Note(() => "Use a real relative endpoint path. Server addresses are configured separately; no request is sent.");
            var feedback = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            form.Root.Add(feedback);
            form.Action("api-save-service", "Create service…", () =>
            {
                if (!ApiServiceId.TryParse(id, out _) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(route))
                { feedback.text = "Enter a valid service ID, display name and relative endpoint path."; return; }
                string path = EditorUtility.SaveFilePanelInProject("Create API service", "ApiServiceDefinition", "asset", "Save the service contract in your project.");
                if (string.IsNullOrEmpty(path)) return;
                if (TryCreate(path, id, name, route, method, out var definition, out string error)) created(definition);
                else feedback.text = error;
            });
        }

        internal static bool TryCreate(string path, string id, string name, string route, HttpMethod method,
            out ApiServiceDefinition definition, out string error)
        {
            definition = null;
            error = "Choose an unused .asset path inside Assets and a valid service ID, name and relative endpoint.";
            if (!DeucarianEditorAssetCatalog.IsUnusedProjectPath(path) || !ApiServiceId.TryParse(id, out _) || string.IsNullOrWhiteSpace(name)) return false;
            var catalog = ScriptableObject.CreateInstance<ApiEndpointCatalog>();
            catalog.CatalogId = id; catalog.DisplayName = name;
            catalog.Endpoints.Add(new ApiEndpointCatalogEntry { EndpointId = "request", ClientId = "primary", RouteTemplate = route,
                Method = method, Authentication = ApiAuthenticationRequirement.Required });
            definition = ApiServiceDefinition.CreateTransient(id, name, catalog,
                ApiEnvironmentStages.All.Select(stage => new ApiEnvironmentDescriptor(
                    new ApiEnvironmentId(stage.ToString().ToLowerInvariant()), stage, stage.ToString())), new[] { new ApiClientId("primary") });
            bool saved = false;
            try
            {
                if (!definition.IsValid(out error)) return false;
                AssetDatabase.CreateAsset(definition, path); saved = true;
                AssetDatabase.AddObjectToAsset(catalog, definition);
                AssetDatabase.SaveAssetIfDirty(definition);
                error = null; return true;
            }
            catch (Exception exception)
            {
                if (saved) AssetDatabase.DeleteAsset(path);
                saved = false;
                error = "Could not create the service (" + exception.GetType().Name + ").";
                return false;
            }
            finally
            {
                if (!saved)
                {
                    if (definition != null && !AssetDatabase.Contains(definition)) Undo.DestroyObjectImmediate(definition);
                    if (catalog != null && !AssetDatabase.Contains(catalog)) Undo.DestroyObjectImmediate(catalog);
                    definition = null;
                }
            }
        }
    }
}
