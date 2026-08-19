using System;
using System.Linq;
using Deucarian.API.Configuration;
using Deucarian.API.Core;
using Deucarian.API.Editor;
using Deucarian.API.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.API.Tests
{
    public sealed class ApiConnectionProfileTests
    {
        private const string TestDirectory =
            "Assets/__DeucarianApiConnectionProfileTests";
        private const string ProfilePath =
            TestDirectory + "/ApiConnectionProfile.asset";
        private const string CatalogPath =
            TestDirectory + "/EndpointCatalog.asset";

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(TestDirectory);
            AssetDatabase.CreateFolder(
                "Assets",
                "__DeucarianApiConnectionProfileTests");
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestDirectory);
            AssetDatabase.Refresh();
        }

        [Test]
        public void Factory_CreatesOneRootWithFourBlankStandardEnvironmentSlots()
        {
            bool created =
                ApiConnectionProfileAssetFactory.TryCreateProjectProfile(
                    ProfilePath,
                    out ApiConnectionProfile profile,
                    out string error);

            Assert.IsTrue(created, error);
            Assert.IsNotNull(profile);
            Assert.IsNull(profile.EndpointCatalog);
            Assert.AreEqual(4, profile.Environments.Count);
            Assert.AreEqual(4, profile.KnownEnvironmentDefinitions.Count);
            CollectionAssert.AreEqual(
                ApiEnvironmentStages.Standard,
                profile.KnownEnvironmentDefinitions
                    .Select(definition => definition.Stage)
                    .ToArray());
            CollectionAssert.AreEqual(
                new[]
                {
                    "development",
                    "testing",
                    "acceptance",
                    "production"
                },
                profile.KnownEnvironmentDefinitions
                    .Select(definition => definition.EnvironmentId)
                    .ToArray());

            foreach (ApiEnvironmentProfile environment in profile.Environments)
            {
                Assert.AreEqual(1, environment.Clients.Count);
                Assert.AreEqual(
                    ApiConnectionProfileAssetFactory.PrimaryClientId,
                    environment.Clients[0].ClientId);
                Assert.IsTrue(
                    string.IsNullOrEmpty(environment.Clients[0].BaseUrl));
                Assert.AreEqual(
                    ApiEnvironmentProfileConfigurationState.NotConfigured,
                    environment.ClassifyConfiguration(out string message));
                Assert.IsNull(message);
            }

            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(ProfilePath);
            Assert.AreEqual(5, assets.Length);
            Assert.AreEqual(
                4,
                assets.OfType<ApiEnvironmentProfile>().Count());
        }

        [Test]
        public void Profile_RoundTripsAndComposesManualCatalogAndHostOverrides()
        {
            Assert.IsTrue(
                ApiConnectionProfileAssetFactory.TryCreateProjectProfile(
                    ProfilePath,
                    out ApiConnectionProfile profile,
                    out string error),
                error);
            ApiEndpointCatalog catalog = CreateCatalogAsset();
            profile.EndpointCatalog = catalog;
            ApiEnvironmentProfile development = profile.Environments[0];
            development.Clients[0].BaseUrl =
                "https://development.example.com/root";
            development.DefaultRequestPolicy.TimeoutSeconds = 41;
            EditorUtility.SetDirty(development);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                ProfilePath,
                ImportAssetOptions.ForceUpdate);

            ApiConnectionProfile reloaded =
                AssetDatabase.LoadAssetAtPath<ApiConnectionProfile>(ProfilePath);
            Assert.IsNotNull(reloaded);
            Assert.AreSame(
                catalog,
                reloaded.EndpointCatalog);
            Assert.IsTrue(
                reloaded.TryCreateComposition(
                    out ApiComposition composition,
                    out string compositionError),
                compositionError);

            ApiResolvedEndpoint endpoint = composition.ResolveEndpoint(
                new ApiEnvironmentId("development"),
                new ApiEndpointId("health.get"));
            Assert.AreEqual(
                "https://development.example.com/root/health",
                endpoint.Endpoint.Path);
            Assert.AreEqual(41, endpoint.RequestPolicy.TimeoutSeconds);
            Assert.AreEqual(
                ApiEnvironmentAvailability.Configured,
                composition.GetEnvironmentStatus("development").Availability);
            Assert.AreEqual(
                ApiEnvironmentAvailability.Unconfigured,
                composition.GetEnvironmentStatus("testing").Availability);
        }

        [Test]
        public void Profile_ExplainsMissingCatalogWithoutResolvingTraffic()
        {
            Assert.IsTrue(
                ApiConnectionProfileAssetFactory.TryCreateProjectProfile(
                    ProfilePath,
                    out ApiConnectionProfile profile,
                    out string error),
                error);

            Assert.IsFalse(
                profile.TryCreateComposition(
                    out ApiComposition composition,
                    out string message));
            Assert.IsNull(composition);
            StringAssert.Contains("Assign an endpoint catalog", message);
        }

        [Test]
        public void DescriptorDefinitions_AreValidatedAndPreserveStageMetadata()
        {
            var valid = new ApiEnvironmentDescriptorDefinition
            {
                EnvironmentId = "vendor.acceptance",
                DisplayName = "Acceptance",
                Stage = ApiEnvironmentStage.Acceptance
            };

            Assert.IsTrue(
                valid.TryCreateDescriptor(
                    out ApiEnvironmentDescriptor descriptor,
                    out string message),
                message);
            Assert.AreEqual(
                new ApiEnvironmentId("vendor.acceptance"),
                descriptor.EnvironmentId);
            Assert.AreEqual(
                ApiEnvironmentStage.Acceptance,
                descriptor.Stage);

            valid.EnvironmentId = "Not A Stable ID";
            Assert.IsFalse(
                valid.TryCreateDescriptor(out descriptor, out message));
            Assert.IsNull(descriptor);
            StringAssert.Contains("invalid stable ID", message);
        }

        [Test]
        public void CatalogOwnership_DistinguishesProjectMissingAndTransientAssets()
        {
            Assert.AreEqual(
                ApiConnectionCatalogOwnership.Missing,
                ApiConnectionProfileEditor.GetCatalogOwnership(null));

            ApiEndpointCatalog projectCatalog = CreateCatalogAsset();
            Assert.AreEqual(
                ApiConnectionCatalogOwnership.ProjectOwned,
                ApiConnectionProfileEditor.GetCatalogOwnership(projectCatalog));

            ApiEndpointCatalog transient =
                ScriptableObject.CreateInstance<ApiEndpointCatalog>();
            try
            {
                Assert.AreEqual(
                    ApiConnectionCatalogOwnership.External,
                    ApiConnectionProfileEditor.GetCatalogOwnership(transient));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(transient);
            }
        }

        [Test]
        public void Inspector_UsesConfiguredNamedClientsWithoutAssumingPrimary()
        {
            ApiEnvironmentProfile environment =
                ScriptableObject.CreateInstance<ApiEnvironmentProfile>();
            try
            {
                environment.Clients.Add(
                    new ApiNamedClientDefinition
                    {
                        ClientId = "vendor.primary",
                        BaseUrl = string.Empty
                    });

                Assert.IsTrue(
                    ApiConnectionProfileEditor.TryGetNamedClients(
                        environment,
                        out var clients));
                Assert.AreEqual(1, clients.Count);
                Assert.AreEqual("vendor.primary", clients[0].ClientId);
                Assert.AreEqual(
                    "Base URL",
                    ApiConnectionProfileEditor.GetBaseUrlLabel(
                        clients[0],
                        clients.Count));

                environment.Clients.Add(
                    new ApiNamedClientDefinition
                    {
                        ClientId = "vendor.media",
                        BaseUrl = string.Empty
                    });
                Assert.AreEqual(
                    "vendor.primary Base URL",
                    ApiConnectionProfileEditor.GetBaseUrlLabel(
                        clients[0],
                        environment.Clients.Count));
                Assert.AreEqual(
                    "vendor.media Base URL",
                    ApiConnectionProfileEditor.GetBaseUrlLabel(
                        environment.Clients[1],
                        environment.Clients.Count));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(environment);
            }
        }

        [Test]
        public void CreationMenus_ExposeOneNormalWorkflowAndAdvancedRawAssets()
        {
            Assert.AreEqual(
                "Assets/Create/Deucarian/API/Connection Profile",
                ApiConnectionProfileAssetFactory.CreateMenuPath);
            AssertCreateAssetMenu<ApiClientConfig>(
                "Deucarian/API/Advanced/Building Blocks/Client Config");
            AssertCreateAssetMenu<ApiEnvironmentProfile>(
                "Deucarian/API/Advanced/Building Blocks/Environment Profile");
            AssertCreateAssetMenu<ApiEndpointCatalog>(
                "Deucarian/API/Advanced/Building Blocks/Endpoint Catalog");
            AssertCreateAssetMenu<ApiEndpointDefinition>(
                "Deucarian/API/Advanced/Building Blocks/Endpoint Definition");
        }

        [Test]
        public void Factory_RejectsPathsOutsideProjectAssets()
        {
            Assert.IsFalse(
                ApiConnectionProfileAssetFactory.TryCreateProjectProfile(
                    "Packages/com.example/Profile.asset",
                    out ApiConnectionProfile profile,
                    out string error));
            Assert.IsNull(profile);
            StringAssert.Contains("inside this project's Assets folder", error);
        }

        private static void AssertCreateAssetMenu<T>(string expected)
        {
            var attribute = (CreateAssetMenuAttribute)Attribute.GetCustomAttribute(
                typeof(T),
                typeof(CreateAssetMenuAttribute));
            Assert.IsNotNull(attribute, typeof(T).Name);
            Assert.AreEqual(expected, attribute.menuName);
        }

        private static ApiEndpointCatalog CreateCatalogAsset()
        {
            ApiEndpointCatalog catalog =
                ScriptableObject.CreateInstance<ApiEndpointCatalog>();
            catalog.CatalogId = "example.v1";
            catalog.DisplayName = "Example API";
            catalog.Endpoints.Add(
                new ApiEndpointCatalogEntry
                {
                    EndpointId = "health.get",
                    ClientId = ApiConnectionProfileAssetFactory.PrimaryClientId,
                    RouteTemplate = "health",
                    Method = HttpMethod.GET,
                    Authentication = ApiAuthenticationRequirement.Required
                });
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }
    }
}
