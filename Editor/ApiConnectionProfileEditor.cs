using System;
using System.Collections.Generic;
using Deucarian.API.Configuration;
using Deucarian.API.Models;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.API.Editor
{
    internal enum ApiConnectionCatalogOwnership
    {
        Missing = 0,
        ProjectOwned = 1,
        PackageManaged = 2,
        External = 3
    }

    [CustomEditor(typeof(ApiConnectionProfile))]
    internal sealed class ApiConnectionProfileEditor : UnityEditor.Editor
    {
        private bool showAdvanced;

        public override void OnInspectorGUI()
        {
            var profile = (ApiConnectionProfile)target;
            bool projectOwned = IsProjectOwned(profile);

            EditorGUILayout.LabelField(
                "API Connection Profile",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Configure project-owned environment hosts here. The endpoint " +
                "catalog defines the shared contract: routes, methods, and " +
                "authentication rules.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            DrawCatalog(profile, projectOwned);
            EditorGUILayout.Space();
            DrawEnvironments(profile, projectOwned);
            EditorGUILayout.Space();
            DrawAdvanced(profile, projectOwned);

            if (!projectOwned)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "This profile is package-managed or transient and is shown " +
                    "read-only. Create a project profile from Assets > Create > " +
                    "Deucarian > API > Connection Profile to configure hosts.",
                    MessageType.Info);
            }
        }

        private void DrawCatalog(
            ApiConnectionProfile profile,
            bool projectOwned)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "API Contract",
                    EditorStyles.boldLabel);
                serializedObject.Update();
                SerializedProperty catalogProperty =
                    serializedObject.FindProperty("endpointCatalog");
                using (new EditorGUI.DisabledScope(!projectOwned))
                {
                    EditorGUILayout.PropertyField(
                        catalogProperty,
                        new GUIContent("Endpoint Catalog"));
                }

                serializedObject.ApplyModifiedProperties();

                ApiConnectionCatalogOwnership ownership =
                    GetCatalogOwnership(profile.EndpointCatalog);
                switch (ownership)
                {
                    case ApiConnectionCatalogOwnership.PackageManaged:
                        DrawState(
                            "Package managed",
                            "Routes, methods, and authentication rules come from " +
                            "the referenced package. Configure only environment " +
                            "hosts below.",
                            DeucarianEditorStatus.Success,
                            MessageType.Info);
                        break;
                    case ApiConnectionCatalogOwnership.ProjectOwned:
                        DrawState(
                            "Project owned",
                            "This project owns both the endpoint contract and its " +
                            "environment hosts.",
                            DeucarianEditorStatus.Info,
                            MessageType.Info);
                        break;
                    case ApiConnectionCatalogOwnership.External:
                        DrawState(
                            "Runtime reference",
                            "The endpoint catalog is not a project or package asset.",
                            DeucarianEditorStatus.Info,
                            MessageType.Info);
                        break;
                    default:
                        DrawState(
                            "Not assigned",
                            "Assign an integration package's catalog or create a " +
                            "project-owned catalog from Advanced > Building " +
                            "Blocks in the API menu.",
                            DeucarianEditorStatus.Warning,
                            MessageType.Warning);
                        break;
                }
            }
        }

        private static void DrawEnvironments(
            ApiConnectionProfile profile,
            bool projectOwned)
        {
            EditorGUILayout.LabelField(
                "Environments",
                EditorStyles.boldLabel);
            if (!profile.TryGetKnownEnvironmentDescriptors(
                    out IReadOnlyList<ApiEnvironmentDescriptor> descriptors,
                    out string descriptorError))
            {
                EditorGUILayout.HelpBox(descriptorError, MessageType.Error);
                return;
            }

            if (descriptors.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No known environments are defined. Use Advanced to attach " +
                    "environment profiles and descriptor metadata.",
                    MessageType.Info);
                return;
            }

            foreach (ApiEnvironmentDescriptor descriptor in descriptors)
            {
                DrawEnvironment(profile, descriptor, projectOwned);
                EditorGUILayout.Space(2f);
            }
        }

        private static void DrawEnvironment(
            ApiConnectionProfile profile,
            ApiEnvironmentDescriptor descriptor,
            bool projectOwned)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        descriptor.DisplayName,
                        EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(
                        descriptor.Stage.ToString(),
                        EditorStyles.miniLabel,
                        GUILayout.Width(90f));
                }

                ApiEnvironmentProfile environment = FindEnvironment(
                    profile.Environments,
                    descriptor.EnvironmentId);
                if (environment == null)
                {
                    DrawState(
                        "Missing slot",
                        "No environment profile is attached for '" +
                        descriptor.EnvironmentId + "'.",
                        DeucarianEditorStatus.Error,
                        MessageType.Error);
                    return;
                }

                if (!environment.TryGetClient(
                        new ApiClientId(
                            ApiConnectionProfileAssetFactory.PrimaryClientId),
                        out ApiNamedClientDefinition client))
                {
                    DrawState(
                        "Advanced setup",
                        "This environment has no 'primary' client. Configure its " +
                        "named clients under Advanced.",
                        DeucarianEditorStatus.Warning,
                        MessageType.Warning);
                    return;
                }

                bool canEdit = projectOwned && IsProjectOwned(environment);
                using (new EditorGUI.DisabledScope(!canEdit))
                {
                    EditorGUI.BeginChangeCheck();
                    string baseUrl = EditorGUILayout.TextField(
                        "Base URL",
                        client.BaseUrl ?? string.Empty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(
                            environment,
                            "Configure API environment host");
                        client.BaseUrl = baseUrl;
                        EditorUtility.SetDirty(environment);
                    }
                }

                ApiEnvironmentProfileConfigurationState state =
                    environment.ClassifyConfiguration(out string message);
                switch (state)
                {
                    case ApiEnvironmentProfileConfigurationState.Configured:
                        DrawState(
                            "Configured",
                            "This environment has a valid absolute HTTP(S) host.",
                            DeucarianEditorStatus.Success,
                            MessageType.Info);
                        break;
                    case ApiEnvironmentProfileConfigurationState.NotConfigured:
                        DrawState(
                            "Not configured",
                            "No requests can resolve here until a Base URL is entered.",
                            DeucarianEditorStatus.Warning,
                            MessageType.Info);
                        break;
                    default:
                        DrawState(
                            "Invalid",
                            message ?? "This environment configuration is invalid.",
                            DeucarianEditorStatus.Error,
                            MessageType.Error);
                        break;
                }
            }
        }

        private void DrawAdvanced(
            ApiConnectionProfile profile,
            bool projectOwned)
        {
            showAdvanced = EditorGUILayout.Foldout(
                showAdvanced,
                "Advanced identifiers and policies",
                true);
            if (!showAdvanced)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "Manual authoring is supported for custom integrations. Keep " +
                "descriptor IDs aligned with their environment profiles and " +
                "never store credentials in headers.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(!projectOwned))
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("knownEnvironmentDefinitions"),
                    new GUIContent("Known Environments"),
                    true);
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("environments"),
                    new GUIContent("Environment Profiles"),
                    true);
                serializedObject.ApplyModifiedProperties();

                foreach (ApiEnvironmentProfile environment in
                         profile.Environments)
                {
                    if (environment == null)
                    {
                        continue;
                    }

                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField(
                        environment.DisplayName ?? environment.name,
                        EditorStyles.boldLabel);
                    var environmentObject = new SerializedObject(environment);
                    environmentObject.Update();
                    EditorGUILayout.PropertyField(
                        environmentObject.FindProperty("defaultRequestPolicy"),
                        new GUIContent("Environment Policy"),
                        true);
                    EditorGUILayout.PropertyField(
                        environmentObject.FindProperty("clients"),
                        new GUIContent("Named Clients"),
                        true);
                    environmentObject.ApplyModifiedProperties();
                }
            }
        }

        private static ApiEnvironmentProfile FindEnvironment(
            IReadOnlyList<ApiEnvironmentProfile> environments,
            ApiEnvironmentId environmentId)
        {
            if (environments != null)
            {
                foreach (ApiEnvironmentProfile environment in environments)
                {
                    if (environment != null &&
                        environment.TryGetId(out ApiEnvironmentId candidate) &&
                        candidate == environmentId)
                    {
                        return environment;
                    }
                }
            }

            return null;
        }

        private static void DrawState(
            string label,
            string message,
            DeucarianEditorStatus status,
            MessageType messageType)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Status", GUILayout.Width(116f));
                DeucarianEditorStatusBadge.Draw(
                    label,
                    status,
                    GUILayout.Width(128f));
            }

            EditorGUILayout.HelpBox(message, messageType);
        }

        internal static ApiConnectionCatalogOwnership GetCatalogOwnership(
            ApiEndpointCatalog catalog)
        {
            if (catalog == null)
            {
                return ApiConnectionCatalogOwnership.Missing;
            }

            string path = AssetDatabase.GetAssetPath(catalog)
                ?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(path))
            {
                return ApiConnectionCatalogOwnership.External;
            }

            if (path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                return ApiConnectionCatalogOwnership.PackageManaged;
            }

            return path.StartsWith("Assets/", StringComparison.Ordinal)
                ? ApiConnectionCatalogOwnership.ProjectOwned
                : ApiConnectionCatalogOwnership.External;
        }

        private static bool IsProjectOwned(UnityEngine.Object value)
        {
            string path = AssetDatabase.GetAssetPath(value)
                ?.Replace('\\', '/');
            return !string.IsNullOrWhiteSpace(path) &&
                   path.StartsWith("Assets/", StringComparison.Ordinal);
        }
    }
}
