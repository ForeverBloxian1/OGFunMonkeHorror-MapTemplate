#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace OGFunMonkeHorror.Editor
{
    public class GuideWindow : EditorWindow
    {
        private enum Tab
        {
            GettingStarted,
            MapRoot,
            SpawnPoints,
            CreatingMaps,
            Visuals,
            AI,
            Exporting,
        }

        private Tab _tab = Tab.GettingStarted;
        private Vector2 _sidebarScroll;
        private Vector2 _contentScroll;

        private static readonly string[] TabLabels =
        {
            "Getting Started",
            "Map Root",
            "Spawn Points",
            "Creating Maps",
            "Visuals",
            "AI",
            "Exporting",
        };

        [MenuItem("OG Fun Monke Horror/Guide")]
        public static void Open()
        {
            var win = GetWindow<GuideWindow>(false, "Guide");
            win.minSize = new Vector2(640f, 480f);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            DrawSidebar();

            var line = GUILayoutUtility.GetRect(1f, position.height, GUILayout.Width(1f));
            EditorGUI.DrawRect(line, new Color(0.15f, 0.15f, 0.15f));

            DrawContent();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(150f));
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Contents", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            for (int i = 0; i < TabLabels.Length; i++)
            {
                var t = (Tab)i;
                bool selected = _tab == t;
                var style = selected ? EditorStyles.boldLabel : EditorStyles.label;
                var r = GUILayoutUtility.GetRect(new GUIContent(TabLabels[i]), style,
                    GUILayout.Height(22f), GUILayout.ExpandWidth(true));

                if (selected)
                    EditorGUI.DrawRect(r, new Color(0.17f, 0.36f, 0.53f, 0.4f));

                EditorGUI.LabelField(r, TabLabels[i], style);

                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                {
                    _tab = t;
                    _contentScroll = Vector2.zero;
                    Repaint();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
            EditorGUILayout.Space(8);

            switch (_tab)
            {
                case Tab.GettingStarted: DrawGettingStarted(); break;
                case Tab.MapRoot: DrawMapRoot(); break;
                case Tab.SpawnPoints: DrawSpawnPoints(); break;
                case Tab.CreatingMaps: DrawCreatingMaps(); break;
                case Tab.Visuals: DrawVisuals(); break;
                case Tab.AI: DrawAI(); break;
                case Tab.Exporting: DrawExporting(); break;
            }

            EditorGUILayout.Space(16);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawGettingStarted()
        {
            Title("Getting Started");
            Note("Thanks for downloading the OG Fun Monke Horror map template. This project has everything you need to build and export custom maps.");
            Section("Quick Steps");
            Body("1. Select the MapRoot GameObject in the Hierarchy.\n2. Fill in your Map Name, Portal Color, and Lighting settings in the Inspector.\n3. Build your level inside MapRoot/Environment.\n4. Export using OG Fun Monke Horror \u2192 Export when you're ready.");
            Section("Need Help?");
            Body("Use the sidebar tabs for documentation on each topic, or reopen this guide any time from OG Fun Monke Horror \u2192 Guide.");
        }

        private void DrawMapRoot()
        {
            Title("Map Root");
            Note("MapRoot is the root GameObject of your map. The MapRoot component holds all the settings for your level.");
            Section("Map Name");
            Body("Your map's display name shown on the portal. Limited to 19 characters.");
            Section("Portal Color");
            Body("The colour of the portal players walk through to load your map.");
            Section("Mods Allowed");
            Body("Controls whether players can use mods while in your map. Enabled by default.");
            Section("Lighting (Skybox)");
            Body("Single Color fills the sky with a flat colour. Single color skyboxes do not appear in the editor and instead will appear ingame.\nMaterial uses a skybox material for outdoor or atmospheric environments.");
            Section("Lighting (Ambient Color)");
            Body("Indirect light filling the whole scene. Defaults to black.\nIf your map uses baked lighting, ambient only affects unbaked objects like the player and moving props.");
            Section("Lighting (Fog)");
            Body("Enable distance fog to obscure far geometry.\n\nStart Distance: where fog begins.\nEnd Distance: where fog is fully opaque.\n\nMatch fog and ambient colours for the best look.\n\nPlease note that fog may have an impact on performance.");
        }

        private void DrawSpawnPoints()
        {
            Title("Spawn Points");
            Section("Player Spawn");
            Body("Add a GameObject named PlayerSpawn as a child of MapRoot. The player appears here when the map loads. Rotate it to set the starting look direction.\n\nYou can add multiple spawn points. If there is more then 1 then the game will choose a random point to spawn the player at.");
        }

        private void DrawCreatingMaps()
        {
            Title("Creating Your Map");
            Section("The Environment Object");
            Body("Inside MapRoot there must be a child named Environment. Everything the player sees should be inside it.");
            EditorGUILayout.HelpBox("Any 3D objects outside MapRoot/Environment will NOT be included in the export. If something is missing in-game, check the Hierarchy first.", MessageType.Warning);
            Section("Default Unity Meshes");
            Body("Unity's built-in primitives are fine for blocking out a layout, but a map made entirely of them will look rough. Use them to prototype, then replace with better geometry.");
            Section("ProBuilder");
            Body("ProBuilder is included with this template. Open it via Tools \u2192 ProBuilder \u2192 ProBuilder Window.\n\nUse New Shape to start building, then select faces, edges, or vertices and extrude or sculpt from there. It's much faster than Blender for architectural shapes.");
            Section("Blender");
            Body("For complex or organic shapes, Blender is the best option. It's free at blender.org.\n\n1. Model your object in Blender.\n2. Apply transforms: Ctrl+A \u2192 All Transforms.\n3. Export as FBX: File \u2192 Export \u2192 FBX.\n4. Drag the FBX into Unity and then into Environment.\n\nKeep polygon counts reasonable for VR. Under 10,000 triangles for large props, under 2,000 for small details.");
            Section("Teleporters");
            Body("A Teleporter lets you teleport the player to one or more points when they walk into a trigger zone.\n\n1. Create an empty GameObject inside Environment and name it Teleporter.\n2. Add a Collider component, Box, Sphere, or Capsule. Do not use a Mesh Collider.\n3. Enable Is Trigger on the Collider.\n4. Set the layer to Ignore Raycast.\n5. Add Component \u2192 Teleporter.\n6. Assign at least one Transform to the Teleport Points list. These are the positions the player can be sent to.\n\nThe Inspector will show errors if any of the required settings are missing. Fix them all before exporting.");
        }

        private void DrawVisuals()
        {
            Title("Visuals");
            Section("URP Post-Processing Volumes");
            Body("A Volume applies full-screen effects to the camera, effects such as bloom, color grading, vignette, etc.\n\n1. Create an empty GameObject inside Visuals, name it Volume.\n2. Add Component \u2192 Rendering \u2192 Volume.\n3. Tick Is Global.\n4. Click New next to Profile.\n5. Click Add Override to add effects.");
            Section("Baked Lighting (Unity)");
            Body("Baked lighting has no runtime cost. Open Window \u2192 Rendering \u2192 Lighting, set lights to Baked or Mixed, mark geometry as Static \u2192 Contribute GI, then click Generate Lighting.");
            Section("Baked Lighting (Bakery) (Recommended)");
            Body("OG Fun Monke Horror uses Bakery, a paid lightmapper with better quality and speed. Find it on the Unity Asset Store.\n\nUse BakeryPointLight, BakeryDirectLight, and BakeryAreaLight instead of Unity lights, then bake from the Bakery window.");
            EditorGUILayout.HelpBox("Bakery requires a modern Nvidia GPU. AMD and Mac are not supported.", MessageType.Warning);
            Section("Realtime Lights");
            Body("Realtime lights are calculated every frame. Use them sparingly.\n\n\u2022 Point Light: emits in all directions.\n\u2022 Spot Light: cone-shaped.\n\u2022 Directional Light: keep as Baked or Mixed.\n\u2022 Area Light: Baked only.\n\nLimit shadow-casting realtime lights to 2-3 per visible area.");
        }

        private void DrawAI()
        {
            Title("AI");
            Note("There are two AI types: Neutral (wanders, ignores the player) and Monster (detects and chases). Both use the MapAI component.");
            Section("Step 1, Bake a NavMesh");
            Body("The NavMeshSurface must be at MapRoot/AI/NavMeshSurface. Select it and click Bake in the Inspector. A blue overlay shows walkable surfaces.");
            Section("Step 2, Add a NavMeshAgent");
            Body("Add Component \u2192 AI \u2192 NavMesh Agent to your AI GameObject. Set Radius and Height to match the character. Leave Speed alone, MapAI sets it at runtime.");
            Section("Step 3, Add MapAI");
            Body("Add Component \u2192 OGFunMonkeHorror \u2192 MapAI and choose your type. The Inspector hides fields that don't apply.");
            Section("Step 4, Waypoints");
            Body("Create empty GameObjects inside MapRoot/Waypoints and position them on walkable floor. Drag them into the Waypoints list on MapAI.\n\nUse at least 4. Every waypoint must be reachable on the NavMesh or the AI will break.");
            Section("Layer Requirement");
            Body("The root GameObject of every AI (both Neutral and Monster) must have its layer set to Ignore Raycast. The export will block if this is not set correctly.");
            Section("Monster Settings");
            Body("\u2022 Chase Distance: detection range in metres.\n\u2022 Field of View: horizontal cone in degrees. 180 is recommended.\n\u2022 Chase Audio: assign an AudioSource with Play On Awake off.\n\nThe monster raycasts toward the player. If nothing blocks the ray inside the FOV cone, the chase begins.");
            Section("Monster Collider");
            Body("Monsters must have a Collider with Is Trigger enabled so the game can detect when the player is caught.\n\nDo not use a Mesh Collider on a Monster. Use a Box, Sphere, or Capsule Collider instead.");
            Section("Scene Gizmos");
            Body("Select a Monster AI in Scene view to see:\n\u2022 Red sphere: Chase Distance.\n\u2022 Orange sphere: extended range.\n\u2022 Yellow cone: Field of View.");
        }

        private void DrawExporting()
        {
            Title("Exporting");
            Note("Maps export as an Android AssetBundle inside a .zip file. Open the exporter from OG Fun Monke Horror \u2192 Export.");
            Section("Steps");
            Body("1. Assign your MapRoot object.\n2. Choose an output folder.\n3. Fix any red errors. Yellow warnings are advisory.\n4. Click Export Map.\n5. Upload the .zip to your mod.io game page.");
            Section("Required");
            Body("\u2022 MapRoot/Environment must exist.\n\u2022 MapRoot/PlayerSpawns must have at least 1 child.\n\u2022 MapRoot/Other/BackButton and MapRoot/Other/Stand must exist.\n\u2022 AI requires a NavMeshSurface and all AI must have waypoints assigned.");
            Section("Warnings");
            Body("\u2022 Environment is empty.\n\u2022 AI exists but no Waypoints group.\n\u2022 Waypoints exist but no AI.\n\u2022 A Volume uses Depth of Field (performance issues on Quest).");
            Section("Visuals Naming");
            Body("\u2022 Volumes must be named Volume and placed directly in Visuals.\n\u2022 Baked lights go in Visuals/Lighting/BakedLights.\n\u2022 Realtime lights go in Visuals/Lighting/RealtimeLights.");
        }

        private void Title(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
            EditorGUILayout.Space(4);
        }

        private void Note(string text)
        {
            EditorGUILayout.HelpBox(text, MessageType.None);
            EditorGUILayout.Space(4);
        }

        private void Section(string text)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        private void Body(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.wordWrappedLabel);
        }
    }
}
#endif