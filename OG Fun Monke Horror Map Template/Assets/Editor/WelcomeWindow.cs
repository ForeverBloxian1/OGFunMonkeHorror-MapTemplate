#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace OGFunMonkeHorror.Editor
{
    public class WelcomeWindow : EditorWindow
    {
        private const string PrefKey = "OGFunMonkeHorror_WelcomeShown";
        private const string PrefSession = "OGFunMonkeHorror_SessionId";

        private Vector2 _scroll;

        [InitializeOnLoadMethod]
        private static void ShowOnStartup()
        {
            string last = SessionState.GetString(PrefSession, "");
            string now = System.Diagnostics.Process.GetCurrentProcess().StartTime.Ticks.ToString();
            if (last == now) return;
            SessionState.SetString(PrefSession, now);
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(PrefKey, false))
                    Open();
            };
        }

        [MenuItem("OG Fun Monke Horror/Welcome")]
        public static void Open()
        {
            var win = GetWindow<WelcomeWindow>(true, "Welcome", true);
            win.minSize = new Vector2(420f, 480f);
            win.maxSize = new Vector2(420f, 480f);
            win.ShowUtility();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("OG Fun Monke Horror Map Template", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Thanks for using the map template.", EditorStyles.miniLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "1. Go into the Scenes folder and open the 'Template1' scene.\n\n" +
                "2. Select the MapRoot GameObject in the Hierarchy.\n\n" +
                "3. Fill in your Map Name, Portal Color, and Lighting in the Inspector.\n\n" +
                "4. Build your level inside MapRoot/Environment.\n\n" +
                "5. Open OG Fun Monke Horror \u2192 Guide for full documentation.",
                MessageType.None);

            EditorGUILayout.Space(4);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Do not show again"))
            {
                EditorPrefs.SetBool(PrefKey, true);
                Close();
            }

            if (GUILayout.Button("OK"))
                Close();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }
    }
}
#endif