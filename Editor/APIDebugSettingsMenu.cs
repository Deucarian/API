using Deucarian.API;
using UnityEditor;

namespace Deucarian.API.Editor
{
    /// <summary>
    /// Toggle APIDebugSettings.LogRawJson from the Unity top menu.
    /// </summary>
    [InitializeOnLoad]
    public static class APIDebugSettingsMenu
    {
        private const string MENU_ROOT = "Tools/Deucarian/Runtime Services/API/";

        private const string MENU_ITEM = MENU_ROOT + "Log Raw JSON";
        private const string PREF_KEY  = "Deucarian.API.LogRawJson";

        static APIDebugSettingsMenu()
        {
            bool storedValue = EditorPrefs.GetBool(PREF_KEY, APIDebugSettings.LogRawJson);
            APIDebugSettings.LogRawJson = storedValue;
        }

        [MenuItem(MENU_ITEM)]
        private static void ToggleLogRawJson()
        {
            bool newValue = !EditorPrefs.GetBool(PREF_KEY, false);
            EditorPrefs.SetBool(PREF_KEY, newValue);
            APIDebugSettings.LogRawJson = newValue;
            ApiLog.General.Info($"APIDebugSettings.LogRawJson is now: {newValue}");
        }

        [MenuItem(MENU_ITEM, true)]
        private static bool ToggleLogRawJsonValidate()
        {
            Menu.SetChecked(MENU_ITEM, EditorPrefs.GetBool(PREF_KEY, false));
            return true;
        }
    }
}
