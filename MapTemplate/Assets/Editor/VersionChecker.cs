#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace OGFunMonkeHorror.Editor
{
    [Serializable]
    public class VersionInfo
    {
        public string version;
        public string downloadUrl;
        public string changelog;
    }

    [InitializeOnLoad]
    public static class VersionChecker
    {
        private const string CurrentVersion    = "1.0.1";
        private const string VersionUrl        = "https://raw.githubusercontent.com/ForeverBloxian1/OGFunMonkeHorror-MapTemplate/refs/heads/main/version.json";
        private const string LastCheckedPref   = "OGFunMonkeHorror_LastVersionCheck";
        private const string SkipVersionPref   = "OGFunMonkeHorror_SkipVersion";
        private const double CheckIntervalHours = 0;

        static VersionChecker()
        {
            EditorApplication.delayCall += Check;
        }

        private static void Check()
        {
            double lastCheck = EditorPrefs.GetFloat(LastCheckedPref, 0);
            double hoursSince = (DateTime.UtcNow - DateTime.UnixEpoch).TotalHours - lastCheck;

            if (hoursSince < CheckIntervalHours) return;

            EditorPrefs.SetFloat(LastCheckedPref, (float)(DateTime.UtcNow - DateTime.UnixEpoch).TotalHours);

            var req = UnityWebRequest.Get(VersionUrl);
            req.SendWebRequest().completed += _ =>
            {
                if (req.result != UnityWebRequest.Result.Success)
                {
                    req.Dispose();
                    return;
                }

                var info = JsonUtility.FromJson<VersionInfo>(req.downloadHandler.text);
                req.Dispose();

                if (info == null || string.IsNullOrEmpty(info.version)) return;
                if (info.version == EditorPrefs.GetString(SkipVersionPref)) return;
                if (!IsNewer(info.version, CurrentVersion)) return;

                int choice = EditorUtility.DisplayDialogComplex(
                    "Update Available",
                    $"A new version of the OG Fun Monke Horror Map Template is available.\n\nInstalled: {CurrentVersion}\nLatest:    {info.version}\n\n{info.changelog}",
                    "Download",
                    "Skip This Version",
                    "Remind Me Later"
                );

                switch (choice)
                {
                    case 0:
                        Application.OpenURL(info.downloadUrl);
                        break;
                    case 1:
                        EditorPrefs.SetString(SkipVersionPref, info.version);
                        break;
                }
            };
        }

        private static bool IsNewer(string latest, string current)
        {
            if (!Version.TryParse(latest, out var l)) return false;
            if (!Version.TryParse(current, out var c)) return false;
            return l > c;
        }

        [MenuItem("OG Fun Monke Horror/Check for Updates")]
        public static void CheckManually()
        {
            EditorPrefs.SetFloat(LastCheckedPref, 0);
            Check();
        }
    }
}
#endif