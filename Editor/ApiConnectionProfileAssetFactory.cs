using System;
using System.Collections.Generic;
using System.IO;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using UnityEditor;
using UnityEngine;

namespace Deucarian.API.Editor
{
    internal static class ApiConnectionProfileAssetFactory
    {
        internal const string CreateMenuPath =
            "Assets/Create/Deucarian/API/Connection Profile";

        internal const string PrimaryClientId = "primary";

        [MenuItem(CreateMenuPath, false, 200)]
        private static void CreateFromMenu()
        {
            string directory = ResolveSelectedProjectDirectory();
            string path = AssetDatabase.GenerateUniqueAssetPath(
                directory + "/ApiConnectionProfile.asset");
            if (!TryCreateProjectProfile(
                    path,
                    out ApiConnectionProfile profile,
                    out string error))
            {
                EditorUtility.DisplayDialog(
                    "Create API Connection Profile",
                    error,
                    "OK");
                return;
            }

            ProjectWindowUtil.ShowCreatedAsset(profile);
        }

        internal static bool TryCreateProjectProfile(
            string assetPath,
            out ApiConnectionProfile profile,
            out string error)
        {
            profile = null;
            if (!TryNormalizeProjectAssetPath(
                    assetPath,
                    out string normalizedPath,
                    out error))
            {
                return false;
            }

            if (AssetDatabase.LoadMainAssetAtPath(normalizedPath) != null)
            {
                error = "An asset already exists at the selected path.";
                return false;
            }

            var environments = new List<ApiEnvironmentProfile>();
            var descriptors = new List<ApiEnvironmentDescriptor>();
            bool createdRootAsset = false;
            try
            {
                foreach (ApiEnvironmentStage stage in
                         ApiEnvironmentStages.Standard)
                {
                    ApiEnvironmentDescriptor descriptor =
                        CreateStandardDescriptor(stage);
                    descriptors.Add(descriptor);
                    environments.Add(CreateEnvironment(descriptor));
                }

                profile = ApiConnectionProfile.CreateTransient(
                    environments,
                    null,
                    descriptors);
                profile.name = Path.GetFileNameWithoutExtension(normalizedPath);
                AssetDatabase.CreateAsset(profile, normalizedPath);
                createdRootAsset = true;
                foreach (ApiEnvironmentProfile environment in environments)
                {
                    AssetDatabase.AddObjectToAsset(environment, profile);
                }

                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(
                    normalizedPath,
                    ImportAssetOptions.ForceUpdate);
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                if (createdRootAsset)
                {
                    AssetDatabase.DeleteAsset(normalizedPath);
                }

                DestroyTransient(profile);
                foreach (ApiEnvironmentProfile environment in environments)
                {
                    DestroyTransient(environment);
                }

                profile = null;
                error = "The API connection profile could not be created (" +
                        exception.GetType().Name + ").";
                return false;
            }
        }

        internal static ApiEnvironmentDescriptor CreateStandardDescriptor(
            ApiEnvironmentStage stage)
        {
            if (stage == ApiEnvironmentStage.Custom)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stage),
                    "A conventional environment stage is required.");
            }

            string displayName = stage.ToString();
            return new ApiEnvironmentDescriptor(
                new ApiEnvironmentId(displayName.ToLowerInvariant()),
                stage,
                displayName);
        }

        private static ApiEnvironmentProfile CreateEnvironment(
            ApiEnvironmentDescriptor descriptor)
        {
            ApiEnvironmentProfile environment =
                ScriptableObject.CreateInstance<ApiEnvironmentProfile>();
            environment.name = descriptor.DisplayName;
            environment.EnvironmentId = descriptor.EnvironmentId.Value;
            environment.DisplayName = descriptor.DisplayName;
            environment.Clients.Add(
                new ApiNamedClientDefinition
                {
                    ClientId = PrimaryClientId,
                    BaseUrl = string.Empty
                });
            return environment;
        }

        private static bool TryNormalizeProjectAssetPath(
            string assetPath,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = assetPath?.Trim().Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(normalizedPath) ||
                !normalizedPath.StartsWith(
                    "Assets/",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    Path.GetExtension(normalizedPath),
                    ".asset",
                    StringComparison.OrdinalIgnoreCase))
            {
                error =
                    "Choose a new .asset path inside this project's Assets folder.";
                normalizedPath = null;
                return false;
            }

            error = null;
            return true;
        }

        private static string ResolveSelectedProjectDirectory()
        {
            string selectedPath = AssetDatabase.GetAssetPath(
                Selection.activeObject);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return "Assets";
            }

            selectedPath = selectedPath.Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(selectedPath))
            {
                selectedPath = Path.GetDirectoryName(selectedPath)
                    ?.Replace('\\', '/');
            }

            return !string.IsNullOrWhiteSpace(selectedPath) &&
                   (string.Equals(
                        selectedPath,
                        "Assets",
                        StringComparison.Ordinal) ||
                    selectedPath.StartsWith(
                        "Assets/",
                        StringComparison.Ordinal))
                ? selectedPath
                : "Assets";
        }

        private static void DestroyTransient(UnityEngine.Object value)
        {
            if (value != null && !AssetDatabase.Contains(value))
            {
                Undo.DestroyObjectImmediate(value);
            }
        }
    }
}
