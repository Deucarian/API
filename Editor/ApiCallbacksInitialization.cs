using Deucarian.API.Core;
using UnityEditor;

namespace Deucarian.API.Editor
{
    /// <summary>Captures Unity's main-thread context for requests started outside Play Mode.</summary>
    [InitializeOnLoad]
    internal static class ApiCallbacksInitialization
    {
        #region Constructors

        static ApiCallbacksInitialization()
        {
            ApiCallbacks.InitializeMainThread();
            EditorApplication.delayCall += ApiCallbacks.InitializeMainThread;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        #endregion

        #region Methods

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                ApiCallbacks.InitializeMainThread();
        }

        #endregion
    }
}
