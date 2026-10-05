#if UNITY_EDITOR
using OGFunMonkeHorror.Assets.AI;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace OGFunMonkeHorror.Editor
{
    public class ExportWindow : EditorWindow
    {
        private enum ExportMode { Single, Multiple }

        private MapRoot _selectedRoot;
        private string _outputFolder = "";
        private Vector2 _scroll;

        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        private ExportMode _mode = ExportMode.Single;
        private readonly List<SceneAsset> _scenes = new();
        private readonly List<string> _multiLog = new();
        private Vector2 _multiLogScroll;
        private bool _multiBusy;

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
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_multiBusy))
            {
                bool singleNow = GUILayout.Toggle(_mode == ExportMode.Single, "Single Export", EditorStyles.miniButtonLeft);
                bool multiNow  = GUILayout.Toggle(_mode == ExportMode.Multiple, "Multiple Export", EditorStyles.miniButtonRight);
                if (singleNow && _mode != ExportMode.Single) _mode = ExportMode.Single;
                else if (multiNow && _mode != ExportMode.Multiple) _mode = ExportMode.Multiple;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Output Folder", string.IsNullOrEmpty(_outputFolder) ? "" : _outputFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string chosen = EditorUtility.OpenFolderPanel("Output Folder", _outputFolder, "");
                if (!string.IsNullOrEmpty(chosen)) _outputFolder = chosen;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            if (_mode == ExportMode.Single)
                DrawSingleMode();
            else
                DrawMultipleMode();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSingleMode()
        {
            EditorGUI.BeginChangeCheck();
            _selectedRoot = (MapRoot)EditorGUILayout.ObjectField("MapRoot", _selectedRoot, typeof(MapRoot), true);
            if (EditorGUI.EndChangeCheck()) RunValidation();

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
            using (new EditorGUI.DisabledScope(_selectedRoot == null || _errors.Count > 0 || string.IsNullOrEmpty(_outputFolder)))
            {
                if (GUILayout.Button("Export", GUILayout.Height(24))) DoExportSingle();
            }
        }

        private void DrawMultipleMode()
        {
            EditorGUILayout.LabelField("Scenes To Export", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Drag a Scene Asset into each row. Each scene must contain a GameObject with a MapRoot component " +
                "at the root of the scene (or as a direct child of a root GameObject). Each scene is exported as " +
                "its own zip into the Output Folder above.",
                MessageType.Info);

            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(_multiBusy))
            {
                for (int i = 0; i < _scenes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _scenes[i] = (SceneAsset)EditorGUILayout.ObjectField(_scenes[i], typeof(SceneAsset), false);
                    if (GUILayout.Button("✕", GUILayout.Width(24)))
                    {
                        _scenes.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Scene", GUILayout.Width(100))) _scenes.Add(null);
                if (_scenes.Count > 0 && GUILayout.Button("Clear", GUILayout.Width(80))) _scenes.Clear();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(12);

            int validSceneCount = 0;
            foreach (var s in _scenes) if (s != null) validSceneCount++;

            using (new EditorGUI.DisabledScope(_multiBusy || validSceneCount == 0 || string.IsNullOrEmpty(_outputFolder)))
            {
                if (GUILayout.Button(_multiBusy ? "Exporting…" : $"Export All ({validSceneCount})", GUILayout.Height(24)))
                    DoExportMultiple();
            }

            if (_multiLog.Count > 0)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField("Export Log", EditorStyles.boldLabel);
                _multiLogScroll = EditorGUILayout.BeginScrollView(_multiLogScroll, GUILayout.MinHeight(120), GUILayout.MaxHeight(280));
                foreach (var line in _multiLog)
                {
                    MessageType mt = MessageType.Info;
                    if (line.StartsWith("[ERROR]")) mt = MessageType.Error;
                    else if (line.StartsWith("[WARN]")) mt = MessageType.Warning;
                    EditorGUILayout.HelpBox(line, mt);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void Update()
        {
            if (_mode == ExportMode.Single && !_multiBusy && _selectedRoot != null)
            {
                RunValidation();
                Repaint();
            }
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

                foreach (var tg in env2.GetComponentsInChildren<ToggleOnTriggered>(true))
                {
                    string tgPath = $"'{tg.gameObject.name}'";
                    var col = tg.GetComponent<Collider>();

                    if (col == null)
                        Error($"ToggleOnTriggered {tgPath} has no Collider.");
                    else
                    {
                        if (!col.isTrigger)
                            Error($"ToggleOnTriggered {tgPath} collider must have Is Trigger enabled.");
                        if (col is MeshCollider mc && !mc.convex)
                            Error($"ToggleOnTriggered {tgPath} MeshCollider must have Convex enabled.");
                    }

                    if (tg.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"ToggleOnTriggered {tgPath} layer must be Ignore Raycast.");

                    if (tg.targets == null || tg.targets.Length == 0)
                        Warn($"ToggleOnTriggered {tgPath} has no Targets assigned, it will do nothing.");
                }

                foreach (var ao in env2.GetComponentsInChildren<AudioOnTriggered>(true))
                {
                    string aoPath = $"'{ao.gameObject.name}'";
                    var col = ao.GetComponent<Collider>();
                    if (col == null) Error($"AudioOnTriggered {aoPath} has no Collider.");
                    else if (!col.isTrigger) Error($"AudioOnTriggered {aoPath} collider must have Is Trigger enabled.");
                    if (ao.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"AudioOnTriggered {aoPath} layer must be Ignore Raycast.");
                    if (ao.clip == null)
                        Warn($"AudioOnTriggered {aoPath} has no Audio Clip assigned.");
                }

                foreach (var sp in env2.GetComponentsInChildren<SpawnOnTriggered>(true))
                {
                    string spPath = $"'{sp.gameObject.name}'";
                    var col = sp.GetComponent<Collider>();
                    if (col == null) Error($"SpawnOnTriggered {spPath} has no Collider.");
                    else if (!col.isTrigger) Error($"SpawnOnTriggered {spPath} collider must have Is Trigger enabled.");
                    if (sp.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"SpawnOnTriggered {spPath} layer must be Ignore Raycast.");
                    if (sp.prefab == null)
                        Warn($"SpawnOnTriggered {spPath} has no Prefab assigned.");
                }

                foreach (var dt in env2.GetComponentsInChildren<DoorTrigger>(true))
                {
                    string dtPath = $"'{dt.gameObject.name}'";
                    var col = dt.GetComponent<BoxCollider>();
                    if (col == null) Error($"DoorTrigger {dtPath} requires a BoxCollider.");
                    else if (!col.isTrigger) Error($"DoorTrigger {dtPath} BoxCollider must have Is Trigger enabled.");
                    if (dt.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                        Error($"DoorTrigger {dtPath} layer must be Ignore Raycast.");
                    if (dt.doorObject == null)
                        Error($"DoorTrigger {dtPath} has no Door Object assigned.");
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

        private void DoExportSingle()
        {
            RunValidation();
            if (_errors.Count > 0)
            {
                EditorUtility.DisplayDialog("Cannot Export",
                    "Fix these errors first:\n\n\u2022 " + string.Join("\n\u2022 ", _errors), "OK");
                Repaint();
                return;
            }

            string err = ExportSceneCore(_selectedRoot, out string zipPath);
            if (err == null)
            {
                EditorUtility.DisplayDialog("Done", $"Exported to:\n{zipPath}", "OK");
                EditorUtility.RevealInFinder(zipPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Export Failed", err, "OK");
            }
        }

        private string ExportSceneCore(MapRoot root, out string zipPath)
        {
            zipPath = null;
            if (root == null) return "MapRoot is null.";

            string safeName = Sanitize(root.MapName);
            string bundleAssetDir = "Assets/OGFMHExport";
            string scenePath = $"{bundleAssetDir}/{safeName}.unity";
            string tempDir = Path.Combine(Path.GetTempPath(), "OGFMHExport_" + safeName);
            zipPath = Path.Combine(_outputFolder, safeName + ".zip");

            string androidBundleName = safeName + "_android";

            try
            {
                if (!AssetDatabase.IsValidFolder(bundleAssetDir))
                    AssetDatabase.CreateFolder("Assets", "OGFMHExport");

                FixNormalMapImporters(root.gameObject);
                AlignBuildSettingsForOgfmh();

                string scriptDataJson = SerializeScriptData(root.gameObject);

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var imp = AssetImporter.GetAtPath(scenePath);
                if (imp == null) return $"Scene asset not found at {scenePath}.";

                string assetsBundleName = safeName + "_assets";
                var assetImporters = new List<AssetImporter>();
                foreach (var ap in CollectAssetBundlePaths(root.gameObject))
                {
                    var ai = AssetImporter.GetAtPath(ap);
                    if (ai == null) continue;
                    ai.assetBundleName = assetsBundleName;
                    assetImporters.Add(ai);
                }
                foreach (var ai in assetImporters) ai.SaveAndReimport();
                bool hasAssets = assetImporters.Count > 0;

                string cacheRoot = Path.Combine(Path.GetTempPath(), "OGFMHBundleCache");
                string androidDir = Path.Combine(cacheRoot, "android");
                Directory.CreateDirectory(androidDir);

                var originalTarget = EditorUserBuildSettings.activeBuildTarget;
                var originalGroup = BuildPipeline.GetBuildTargetGroup(originalTarget);
                try
                {
                    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                    imp.assetBundleName = androidBundleName;
                    imp.SaveAndReimport();
                    BuildPipeline.BuildAssetBundles(androidDir,
                        BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
                }
                finally
                {
                    imp.assetBundleName = "";
                    imp.SaveAndReimport();
                    foreach (var ai in assetImporters) ai.assetBundleName = "";
                    foreach (var ai in assetImporters) ai.SaveAndReimport();
                    if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                        EditorUserBuildSettings.SwitchActiveBuildTarget(originalGroup, originalTarget);
                }

                string androidFile = Path.Combine(androidDir, androidBundleName);
                if (!File.Exists(androidFile)) return "The Quest bundle wasn't created.";

                string androidAssetsEntry = assetsBundleName + "_android";
                string androidAssetsFile = Path.Combine(androidDir, assetsBundleName);
                bool haveAndroidAssets = hasAssets && File.Exists(androidAssetsFile);

                var meta = new MapMeta
                {
                    mapName = root.MapName,
                    androidBundle = androidBundleName,
                    androidAssetsBundle = haveAndroidAssets ? androidAssetsEntry : ""
                };
                meta.PortalColor = root.PortalColor;

                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                string jsonPath = Path.Combine(tempDir, "map.json");
                File.WriteAllText(jsonPath, JsonUtility.ToJson(meta, true));

                if (File.Exists(zipPath)) File.Delete(zipPath);
                string scriptDataOutputPath = Path.Combine(tempDir, "scriptdata.json");
                File.WriteAllText(scriptDataOutputPath, scriptDataJson);

                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(androidFile, androidBundleName);
                    if (haveAndroidAssets) zip.CreateEntryFromFile(androidAssetsFile, androidAssetsEntry);
                    zip.CreateEntryFromFile(jsonPath, "map.json");
                    zip.CreateEntryFromFile(scriptDataOutputPath, "scriptdata.json");
                }

                Directory.Delete(tempDir, true);

                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Export failed: " + ex);
                return ex.Message;
            }
        }

        private void DoExportMultiple()
        {
            _multiLog.Clear();
            _multiBusy = true;

            string originalScenePath = SceneManager.GetActiveScene().path;
            bool originalIsDirty = SceneManager.GetActiveScene().isDirty;

            if (originalIsDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    _multiLog.Add("[ERROR] Export cancelled because the current scene has unsaved changes.");
                    _multiBusy = false;
                    Repaint();
                    return;
                }
            }

            int success = 0;
            int failed = 0;

            try
            {
                for (int i = 0; i < _scenes.Count; i++)
                {
                    var sceneAsset = _scenes[i];
                    if (sceneAsset == null) continue;

                    string sceneName = sceneAsset.name;
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);

                    _multiLog.Add($"--- {sceneName} ---");
                    Repaint();

                    try { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
                    catch (System.Exception ex)
                    {
                        _multiLog.Add($"[ERROR] {sceneName}: failed to open scene: {ex.Message}");
                        failed++;
                        continue;
                    }

                    MapRoot foundRoot = null;
                    foreach (var rootGo in SceneManager.GetActiveScene().GetRootGameObjects())
                    {
                        foundRoot = rootGo.GetComponentInChildren<MapRoot>(true);
                        if (foundRoot != null) break;
                    }

                    if (foundRoot == null)
                    {
                        _multiLog.Add($"[ERROR] {sceneName}: no MapRoot found in scene.");
                        failed++;
                        continue;
                    }

                    _errors.Clear();
                    _warnings.Clear();
                    Validate(foundRoot.transform);

                    foreach (var w in _warnings) _multiLog.Add($"[WARN] {sceneName}: {w}");

                    if (_errors.Count > 0)
                    {
                        foreach (var e in _errors) _multiLog.Add($"[ERROR] {sceneName}: {e}");
                        _multiLog.Add($"[ERROR] {sceneName}: skipped due to {_errors.Count} validation error(s).");
                        failed++;
                        continue;
                    }

                    string err = ExportSceneCore(foundRoot, out string zipPath);
                    if (err == null)
                    {
                        _multiLog.Add($"{sceneName}: exported OK \u2192 {zipPath}");
                        success++;
                    }
                    else
                    {
                        _multiLog.Add($"[ERROR] {sceneName}: export failed: {err}");
                        failed++;
                    }

                    Repaint();
                }

                _multiLog.Add($"=== Done: {success} succeeded, {failed} failed ===");
            }
            finally
            {
                if (!string.IsNullOrEmpty(originalScenePath))
                {
                    try { EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single); }
                    catch { }
                }

                _multiBusy = false;
                Repaint();
            }

            EditorUtility.DisplayDialog("Multiple Export",
                $"{success} scene(s) exported successfully.\n{failed} scene(s) failed.\n\nSee the export log for per-scene details.",
                "OK");

            if (success > 0 && !string.IsNullOrEmpty(_outputFolder))
                EditorUtility.RevealInFinder(_outputFolder);
        }

        private static List<string> CollectAssetBundlePaths(GameObject mapRoot)
        {
            var paths = new HashSet<string>();

            var mh = mapRoot.GetComponentInChildren<MapHitsounds>(true);
            if (mh != null && mh.materialSounds != null)
                foreach (var ms in mh.materialSounds)
                {
                    AddAssetPaths(paths, ms.materials);
                    AddAssetPaths(paths, ms.sounds);
                }

            foreach (var script in mapRoot.GetComponentsInChildren<MapScript>(true))
            {
                if (script.fields == null) continue;
                foreach (var f in script.fields)
                {
                    if (f == null || f.objectValue == null) continue;
                    if (!MapScriptFields.IsObjectType(f.type)) continue;
                    Transform tr = (f.objectValue as GameObject)?.transform ?? (f.objectValue as Component)?.transform;
                    bool sceneObject = tr != null && tr.gameObject.scene.IsValid();
                    if (MapScriptFields.AllowSceneObject(f.type) && sceneObject) continue;
                    AddAssetPath(paths, f.objectValue);
                }
            }

            return new List<string>(paths);
        }

        private static void AddAssetPaths(HashSet<string> paths, UnityEngine.Object[] objs)
        {
            if (objs == null) return;
            foreach (var o in objs) AddAssetPath(paths, o);
        }

        private static void AddAssetPath(HashSet<string> paths, UnityEngine.Object o)
        {
            if (o == null || !EditorUtility.IsPersistent(o)) return;
            string p = AssetDatabase.GetAssetPath(o);
            if (string.IsNullOrEmpty(p)) return;
            if (p.StartsWith("Packages/")) return;
            if (p.EndsWith(".unity")) return;
            paths.Add(p);
        }

        private static void AlignBuildSettingsForOgfmh()
        {
            var nbt = NamedBuildTarget.Android;

            if (PlayerSettings.GetNormalMapEncoding(nbt) != NormalMapEncoding.XYZ)
                PlayerSettings.SetNormalMapEncoding(nbt, NormalMapEncoding.XYZ);

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
                PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
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
                    maxPlayers = mr.MaxPlayers,
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

            foreach (var tg in mapRoot.GetComponentsInChildren<ToggleOnTriggered>(true))
            {
                var targetPaths = new string[tg.targets?.Length ?? 0];
                for (int i = 0; i < targetPaths.Length; i++)
                    targetPaths[i] = tg.targets[i] != null ? GetObjectPath(tg.targets[i].transform) : "";

                data.togglers.Add(new ToggleData
                {
                    objectPath = GetObjectPath(tg.transform),
                    targetPaths = targetPaths,
                    mode = (int)tg.mode,
                    oneShot = tg.oneShot,
                });
            }

            foreach (var dt in mapRoot.GetComponentsInChildren<DoorTrigger>(true))
            {
                data.doors.Add(new DoorData
                {
                    objectPath = GetObjectPath(dt.transform),
                    doorObjectPath = dt.doorObject != null ? GetObjectPath(dt.doorObject) : "",
                    openPosX = dt.openPosition.x,
                    openPosY = dt.openPosition.y,
                    openPosZ = dt.openPosition.z,
                    closedPosX = dt.closedPosition.x,
                    closedPosY = dt.closedPosition.y,
                    closedPosZ = dt.closedPosition.z,
                    speed = dt.speed,
                    autoOpen = dt.autoOpen,
                    autoOpenTimer = dt.autoOpenTimer,
                    networkMode = (int)dt.networkMode,
                });
            }

            foreach (var ms in mapRoot.GetComponentsInChildren<MapScript>(true))
            {
                if (ms.script == null) continue;

                var fields = new List<LuaFieldData>();
                if (ms.fields != null)
                {
                    foreach (var f in ms.fields)
                    {
                        if (f == null || string.IsNullOrEmpty(f.name)) continue;
                        var fd = new LuaFieldData { name = f.name, type = f.type };

                        if (MapScriptFields.IsObjectType(f.type))
                        {
                            Transform tr = (f.objectValue as GameObject)?.transform ?? (f.objectValue as Component)?.transform;
                            bool sceneObject = tr != null && tr.gameObject.scene.IsValid();
                            if (MapScriptFields.AllowSceneObject(f.type) && sceneObject)
                                fd.objectPath = GetObjectPath(tr);
                            else if (f.objectValue != null)
                                fd.assetName = f.objectValue.name;
                        }
                        else if (f.type == "number") fd.number = f.numberValue;
                        else if (f.type == "bool") fd.boolean = f.boolValue;
                        else fd.text = f.stringValue;

                        fields.Add(fd);
                    }
                }

                data.luaScripts.Add(new LuaScriptData
                {
                    objectPath = GetObjectPath(ms.transform),
                    scriptName = ms.script.name,
                    source = ms.script.Text,
                    fields = fields,
                });
            }

            var mapHitsounds = mapRoot.GetComponentInChildren<MapHitsounds>(true);
            if (mapHitsounds != null && mapHitsounds.materialSounds != null)
            {
                foreach (var msnd in mapHitsounds.materialSounds)
                {
                    data.hitsounds.entries.Add(new HitsoundEntry
                    {
                        materialNames = AssetNames(msnd.materials),
                        soundNames = AssetNames(msnd.sounds),
                    });
                }
            }

            return JsonUtility.ToJson(data, true);
        }

        private static string[] AssetNames(UnityEngine.Object[] assets)
        {
            if (assets == null) return new string[0];
            var names = new string[assets.Length];
            for (int i = 0; i < assets.Length; i++)
                names[i] = assets[i] != null ? assets[i].name : "";
            return names;
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