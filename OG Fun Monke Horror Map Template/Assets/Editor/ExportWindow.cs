#if UNITY_EDITOR
using OGFunMonkeHorror.Assets.AI;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace OGFunMonkeHorror.Editor
{
    public class ExportWindow : EditorWindow
    {
        private MapRoot _selectedRoot;
        private string _outputFolder = "";
        private Vector2 _scroll;

        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        [MenuItem("OG Fun Monke Horror/Export")]
        public static void Open()
        {
            var win = GetWindow<ExportWindow>(false, "Export Map");
            win.minSize = new Vector2(420f, 520f);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Export Map", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            _selectedRoot = (MapRoot)EditorGUILayout.ObjectField("MapRoot", _selectedRoot, typeof(MapRoot), true);
            if (EditorGUI.EndChangeCheck()) RunValidation();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Output Folder", string.IsNullOrEmpty(_outputFolder) ? "" : _outputFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string chosen = EditorUtility.OpenFolderPanel("Output Folder", _outputFolder, "");
                if (!string.IsNullOrEmpty(chosen)) _outputFolder = chosen;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            if (_selectedRoot == null)
                EditorGUILayout.HelpBox("Assign a MapRoot object to validate your map.", MessageType.Info);
            else if (_errors.Count == 0 && _warnings.Count == 0)
                EditorGUILayout.HelpBox("All checks passed.", MessageType.Info);
            else
            {
                foreach (var e in _errors) EditorGUILayout.HelpBox(e, MessageType.Error);
                foreach (var w in _warnings) EditorGUILayout.HelpBox(w, MessageType.Warning);
            }

            EditorGUILayout.Space(12);
            EditorGUI.BeginDisabledGroup(_selectedRoot == null || _errors.Count > 0 || string.IsNullOrEmpty(_outputFolder));
            if (GUILayout.Button("Export", GUILayout.Height(24))) DoExport();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            if (_selectedRoot != null) { RunValidation(); Repaint(); }
        }

        private void RunValidation()
        {
            _errors.Clear();
            _warnings.Clear();
            if (_selectedRoot == null) return;
            Validate(_selectedRoot.transform);
        }

        private void Validate(Transform root)
        {
            var env = root.Find("Environment");
            if (env == null) Error("Missing: MapRoot/Environment");
            else if (env.childCount == 0) Warn("Environment exists but is empty.");

            var spawns = root.Find("PlayerSpawns");
            if (spawns == null) Error("Missing: MapRoot/PlayerSpawns");
            else if (spawns.childCount == 0) Error("MapRoot/PlayerSpawns has no spawn points.");

            if (root.Find("Other") == null) Error("Missing: MapRoot/Other");
            else
            {
                if (root.Find("Other/BackButton") == null) Error("Missing: MapRoot/Other/BackButton");
                if (root.Find("Other/Stand") == null) Error("Missing: MapRoot/Other/Stand");
            }

            var allAI = root.GetComponentsInChildren<MapAI>();
            var waypointsGo = root.Find("Waypoints");
            bool hasAI = allAI != null && allAI.Length > 0;
            bool hasWaypoints = waypointsGo != null && waypointsGo.childCount > 0;

            if (hasAI)
            {
                if (root.GetComponentInChildren<NavMeshSurface>() == null)
                    Error("AI is present but no NavMeshSurface was found.");

                foreach (var ai in allAI)
                {
                    string aiName = $"'{ai.gameObject.name}'";

                    if (ai.waypoints == null || ai.waypoints.Length == 0)
                        Error($"{aiName} has no waypoints assigned.");

                    if (ai.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"AI {aiName} layer must be set to Ignore Raycast.");

                    if (ai.aiType == MapAI.AIType.Monster)
                    {
                        var col = ai.GetComponent<Collider>();
                        if (col == null)
                            Error($"Monster {aiName} must have a Collider with Is Trigger enabled.");
                        else
                        {
                            if (!col.isTrigger)
                                Error($"Monster {aiName} collider must have Is Trigger enabled.");
                            if (col is MeshCollider)
                                Error($"Monster {aiName} cannot use a Mesh Collider. Use Box, Sphere, or Capsule.");
                        }

                        var js = ai.GetComponent<Jumpscare>();
                        if (js == null)
                            Warn($"Monster {aiName} has no Jumpscare script assigned.");
                        else
                        {
                            if (js.jumpscarePrefab == null)
                                Error($"Monster {aiName} Jumpscare has no prefab assigned.");
                            if (js.respawnPoints == null || js.respawnPoints.Length == 0)
                                Error($"Monster {aiName} Jumpscare has no respawn points assigned.");
                        }
                    }
                }

                if (!hasWaypoints) Warn("AI found but no Waypoints group in the scene.");
            }

            if (hasWaypoints && !hasAI) Warn("Waypoints group exists but no AI was found.");

            var env2 = root.Find("Environment");
            if (env2 != null)
            {
                foreach (var tp in env2.GetComponentsInChildren<Teleporter>(true))
                {
                    string tpPath = $"'{tp.gameObject.name}'";
                    var col = tp.GetComponent<Collider>();

                    if (col == null)
                        Error($"Teleporter {tpPath} has no Collider.");
                    else
                    {
                        if (!col.isTrigger)
                            Error($"Teleporter {tpPath} collider must have Is Trigger enabled.");
                        if (col is MeshCollider mc && !mc.convex)
                            Error($"Teleporter {tpPath} MeshCollider must have Convex enabled.");
                    }

                    if (tp.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"Teleporter {tpPath} layer must be Ignore Raycast.");

                    if (tp.teleportPoints == null || tp.teleportPoints.Length == 0)
                        Error($"Teleporter {tpPath} must have at least 1 Teleport Point assigned.");
                }
            }

            var visuals = root.Find("Visuals");
            if (visuals != null)
            {
                for (int i = 0; i < visuals.childCount; i++)
                {
                    var child = visuals.GetChild(i);
                    if (child.GetComponent<Volume>() != null && child.name != "Volume")
                        Error($"Volume '{child.name}' must be named 'Volume' to work in-game.");
                }

                var lightingT = visuals.Find("Lighting");
                var bakedLightsT = visuals.Find("Lighting/BakedLights");
                var rtLightsT = visuals.Find("Lighting/RealtimeLights");

                if (lightingT != null)
                {
                    foreach (var lt in root.GetComponentsInChildren<Light>())
                    {
                        if (!lt.transform.IsChildOf(lightingT)) continue;
                        if (lt.lightmapBakeType == LightmapBakeType.Baked
                            && (bakedLightsT == null || !lt.transform.IsChildOf(bakedLightsT)))
                            Error($"Baked light '{lt.gameObject.name}' must be in Visuals/Lighting/BakedLights.");
                        if (lt.lightmapBakeType == LightmapBakeType.Realtime
                            && (rtLightsT == null || !lt.transform.IsChildOf(rtLightsT)))
                            Error($"Realtime light '{lt.gameObject.name}' must be in Visuals/Lighting/RealtimeLights.");
                    }
                }

                foreach (var vol in root.GetComponentsInChildren<Volume>())
                    if (vol.profile != null && vol.profile.Has<DepthOfField>())
                        Warn($"'{vol.gameObject.name}' uses Depth of Field which can cause performance issues on Quest.");
            }
        }

        private void DoExport()
        {
            RunValidation();
            if (_errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Cannot Export",
                    "Fix these errors first:\n\n\u2022 " + string.Join("\n\u2022 ", _errors), "OK");
                Repaint();
                return;
            }

            string safeName = Sanitize(_selectedRoot.MapName);
            string bundleAssetDir = "Assets/OGFMHExport";
            string scenePath = $"{bundleAssetDir}/{safeName}.unity";
            string tempDir = Path.Combine(Path.GetTempPath(), "OGFMHExport_" + safeName);
            string zipPath = Path.Combine(_outputFolder, safeName + ".zip");

            string androidBundleName = safeName + "_android";
            string win64BundleName = safeName + "_win64";

            try
            {
                if (!AssetDatabase.IsValidFolder(bundleAssetDir))
                    AssetDatabase.CreateFolder("Assets", "OGFMHExport");

                FixNormalMapImporters(_selectedRoot.gameObject);

                string scriptDataJson = SerializeScriptData(_selectedRoot.gameObject);

                string activeScenePath = SceneManager.GetActiveScene().path;
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var imp = AssetImporter.GetAtPath(scenePath);
                if (imp == null) throw new System.Exception($"Scene asset not found at {scenePath}.");

                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                imp.assetBundleName = androidBundleName;
                imp.SaveAndReimport();

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                BuildPipeline.BuildAssetBundles(tempDir,
                    BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);

                imp.assetBundleName = win64BundleName;
                imp.SaveAndReimport();

                string win64TempDir = tempDir + "_win64";
                if (Directory.Exists(win64TempDir)) Directory.Delete(win64TempDir, true);
                Directory.CreateDirectory(win64TempDir);

                BuildPipeline.BuildAssetBundles(win64TempDir,
                    BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

                imp.assetBundleName = "";
                imp.SaveAndReimport();

                string androidFile = Path.Combine(tempDir, androidBundleName);
                string win64File = Path.Combine(win64TempDir, win64BundleName);

                if (!File.Exists(androidFile))
                    throw new System.Exception("Android bundle was not produced.");
                if (!File.Exists(win64File))
                    throw new System.Exception("Win64 bundle was not produced.");

                var meta = new MapMeta
                {
                    mapName = _selectedRoot.MapName,
                    androidBundle = androidBundleName,
                    win64Bundle = win64BundleName
                };
                meta.PortalColor = _selectedRoot.PortalColor;

                string jsonPath = Path.Combine(tempDir, "map.json");
                File.WriteAllText(jsonPath, JsonUtility.ToJson(meta, true));

                if (File.Exists(zipPath)) File.Delete(zipPath);
                string scriptDataOutputPath = Path.Combine(tempDir, "scriptdata.json");
                File.WriteAllText(scriptDataOutputPath, scriptDataJson);

                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(androidFile, androidBundleName);
                    zip.CreateEntryFromFile(win64File, win64BundleName);
                    zip.CreateEntryFromFile(jsonPath, "map.json");
                    zip.CreateEntryFromFile(scriptDataOutputPath, "scriptdata.json");
                }

                Directory.Delete(tempDir, true);
                Directory.Delete(win64TempDir, true);

                EditorUtility.DisplayDialog("Done", $"Exported to:\n{zipPath}", "OK");
                EditorUtility.RevealInFinder(zipPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Export failed: " + ex);
                EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
            }
        }

        private static readonly string[] NormalMapProps = { "_BumpMap", "_NormalMap", "_DetailNormalMap" };

        private static void FixNormalMapImporters(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null) continue;
                    foreach (string prop in NormalMapProps)
                    {
                        if (!mat.HasProperty(prop)) continue;
                        var tex = mat.GetTexture(prop);
                        if (tex == null) continue;
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (string.IsNullOrEmpty(path)) continue;
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer == null) continue;
                        if (importer.textureType != TextureImporterType.NormalMap)
                        {
                            importer.textureType = TextureImporterType.NormalMap;
                            importer.SaveAndReimport();
                        }
                    }
                }
            }
        }

        private static string SerializeScriptData(GameObject mapRoot)
        {
            var data = new MapScriptData();

            var mr = mapRoot.GetComponent<MapRoot>();
            if (mr != null)
            {
                data.mapRoots.Add(new MapRootData
                {
                    objectPath = GetObjectPath(mr.transform),
                    mapName = mr.MapName,
                    portalColorR = mr.PortalColor.r,
                    portalColorG = mr.PortalColor.g,
                    portalColorB = mr.PortalColor.b,
                    portalColorA = mr.PortalColor.a,
                    modsAllowed = mr.ModsAllowed,
                    skyboxMode = (int)mr.SkyboxModeValue,
                    skyboxMaterialName = mr.SkyboxMaterial != null ? mr.SkyboxMaterial.name : "",
                    skyboxColorR = mr.SkyboxColor.r,
                    skyboxColorG = mr.SkyboxColor.g,
                    skyboxColorB = mr.SkyboxColor.b,
                    ambientColorR = mr.AmbientColor.r,
                    ambientColorG = mr.AmbientColor.g,
                    ambientColorB = mr.AmbientColor.b,
                    fogEnabled = mr.FogEnabled,
                    fogColorR = mr.FogColor.r,
                    fogColorG = mr.FogColor.g,
                    fogColorB = mr.FogColor.b,
                    fogStart = mr.FogStart,
                    fogEnd = mr.FogEnd,
                });
            }

            foreach (var ai in mapRoot.GetComponentsInChildren<MapAI>(true))
            {
                var waypointPaths = new string[ai.waypoints?.Length ?? 0];
                for (int i = 0; i < waypointPaths.Length; i++)
                    waypointPaths[i] = ai.waypoints[i] != null ? GetObjectPath(ai.waypoints[i].transform) : "";

                data.mapAIs.Add(new MapAIData
                {
                    objectPath = GetObjectPath(ai.transform),
                    aiType = (int)ai.aiType,
                    wanderSpeed = ai.wanderSpeed,
                    chaseSpeed = ai.chaseSpeed,
                    chaseDistance = ai.chaseDistance,
                    fieldOfViewAngle = ai.fieldOfViewAngle,
                    waypointPaths = waypointPaths,
                });
            }

            foreach (var tp in mapRoot.GetComponentsInChildren<Teleporter>(true))
            {
                var pointPaths = new string[tp.teleportPoints?.Length ?? 0];
                for (int i = 0; i < pointPaths.Length; i++)
                    pointPaths[i] = tp.teleportPoints[i] != null ? GetObjectPath(tp.teleportPoints[i]) : "";

                data.teleporters.Add(new TeleporterData
                {
                    objectPath = GetObjectPath(tp.transform),
                    teleportPointPaths = pointPaths,
                });
            }

            foreach (var js in mapRoot.GetComponentsInChildren<Jumpscare>(true))
            {
                var pointPaths = new string[js.respawnPoints?.Length ?? 0];
                for (int i = 0; i < pointPaths.Length; i++)
                    pointPaths[i] = js.respawnPoints[i] != null ? GetObjectPath(js.respawnPoints[i]) : "";

                data.jumpscares.Add(new JumpscareData
                {
                    objectPath = GetObjectPath(js.transform),
                    respawnPaths = pointPaths,
                });
            }

            return JsonUtility.ToJson(data, true);
        }

        private static string GetObjectPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private void Error(string msg) => _errors.Add(msg);
        private void Warn(string msg) => _warnings.Add(msg);

        private static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name)) name = "map";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_').ToLower();
        }
    }
}
#endif