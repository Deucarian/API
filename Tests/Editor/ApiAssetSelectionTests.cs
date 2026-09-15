using System.Collections;
using Deucarian.API.Configuration;
using Deucarian.API.Editor;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.API.Tests
{
    public sealed class ApiAssetSelectionTests
    {
        private sealed class Window : EditorWindow { }

        [Test]
        public void CustomServiceRoundTripsItsOwnedCatalogAndCanBeCustomizedIndependently()
        {
            const string path = "Assets/__CreatedApiService.asset";
            const string copyPath = "Assets/__CopiedApiService.asset";
            Assert.IsTrue(Deucarian.Editor.DeucarianEditorAssetCatalog.IsUnusedProjectPath(path));
            Assert.IsTrue(Deucarian.Editor.DeucarianEditorAssetCatalog.IsUnusedProjectPath(copyPath));
            try
            {
                Assert.IsTrue(ApiServiceDefinitionAuthoring.TryCreate(path, "example.api", "Example service",
                    "/items/{id}", HttpMethod.GET, out var source, out string error), error);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                source = AssetDatabase.LoadAssetAtPath<ApiServiceDefinition>(path);
                Assert.IsTrue(source.IsValid(out error), error);
                Assert.AreEqual(ApiEnvironmentStages.All.Count, source.KnownEnvironments.Count);
                Assert.IsTrue(AssetDatabase.IsSubAsset(source.EndpointCatalog));
                Assert.AreEqual(path, AssetDatabase.GetAssetPath(source.EndpointCatalog));
                Assert.AreEqual("/items/{id}", source.EndpointCatalog.Endpoints[0].RouteTemplate);
                Assert.IsFalse(ApiServiceDefinitionAuthoring.TryCreate(path, "overwrite.api", "Overwrite",
                    "/items", HttpMethod.GET, out _, out _));
                Assert.AreEqual("example.api", source.ServiceId);
                var copy = ApiServiceDefinitionAuthoring.Copy(source, copyPath);
                AssetDatabase.ImportAsset(copyPath, ImportAssetOptions.ForceUpdate);
                copy = AssetDatabase.LoadAssetAtPath<ApiServiceDefinition>(copyPath);
                Assert.AreNotSame(source.EndpointCatalog, copy.EndpointCatalog);
                Assert.AreEqual(copyPath, AssetDatabase.GetAssetPath(copy.EndpointCatalog));
                copy.EndpointCatalog.Endpoints[0].RouteTemplate = "/different";
                Assert.AreEqual("/items/{id}", source.EndpointCatalog.Endpoints[0].RouteTemplate);
            }
            finally { AssetDatabase.DeleteAsset(copyPath); AssetDatabase.DeleteAsset(path); }
        }

        [UnityTest]
        public IEnumerator ServiceAndSettingsPickersRemainAttachedAfterEverySelection()
        {
            var window = ScriptableObject.CreateInstance<Window>(); window.Show();
            var a = ScriptableObject.CreateInstance<ApiServiceDefinition>();
            var b = ScriptableObject.CreateInstance<ApiServiceDefinition>();
            var c = ScriptableObject.CreateInstance<ApiConnectionSettings>();
            var d = ScriptableObject.CreateInstance<ApiConnectionSettings>();
            try
            {
                using (var page = ApiConnectionsWindow.CreatePage())
                {
                    window.rootVisualElement.Add(page.Root);
                    yield return null;
                    var add = page.Root.Query<Button>().ToList().Find(button => button.text == "+ Add connection");
                    Assert.NotNull(add);
                    add.Focus(); yield return null;
                    using (var click = NavigationSubmitEvent.GetPooled()) { click.target = add; add.SendEvent(click); }
                    yield return null;
                    var service = page.Root.Q<ObjectField>("api-add-definition");
                    var settings = page.Root.Q<ObjectField>("api-add-settings");
                    Assert.NotNull(service); Assert.NotNull(settings);
                    foreach (var value in new Object[] { a, b, null })
                    { service.value = value; Assert.AreSame(service, page.Root.Q<ObjectField>("api-add-definition")); Assert.NotNull(service.panel); }
                    foreach (var value in new Object[] { c, d, null })
                    { settings.value = value; Assert.AreSame(settings, page.Root.Q<ObjectField>("api-add-settings")); Assert.NotNull(settings.panel); }
                    Assert.IsFalse(page.Root.Q<Button>("api-bind-settings").enabledSelf);
                }
            }
            finally { window.Close(); Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(c); Object.DestroyImmediate(d); }
        }

        [Test]
        public void CustomServiceRejectsAnInventedAbsoluteEndpointWithoutCreatingAnAsset()
        {
            const string path = "Assets/__RejectedApiService.asset";
            Assert.IsFalse(ApiServiceDefinitionAuthoring.TryCreate(path, "example.api", "Example", "https://example.invalid", HttpMethod.GET, out var asset, out _));
            Assert.IsNull(asset); Assert.IsNull(AssetDatabase.LoadMainAssetAtPath(path));
        }
    }
}
