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
            Triggers,
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
            "Triggers",
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
                case Tab.Triggers: DrawTriggers(); break;
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
            Note("This template contains everything required to build and export a custom map for OG Fun Monke Horror. The tabs on the left walk through each part of the workflow in the order you'll need them.");

            Section("Overview");
            Body("A map is a Unity scene built around a single GameObject called MapRoot. MapRoot acts as the container for everything your level contains. Geometry, spawn points, lighting, AI, and triggers. When you export, the entire MapRoot and its children are packaged into an AssetBundle that the main game can load at runtime.");

            Section("Workflow Summary");
            Body("1. Open the example scene under Assets/Scenes. It already has a MapRoot prefab set up with the required child structure.\n2. Select MapRoot and fill in the map's name, portal color, and lighting settings in the Inspector.\n3. Build your level geometry inside MapRoot/Environment.\n4. Add triggers, AI, and post-processing as needed (covered in their own tabs).\n5. Bake your lighting from Window → Rendering → Lighting.\n6. Export from OG Fun Monke Horror → Export in the top menu.");

            Section("How the Exporter Helps");
            Body("The exporter scans your scene for missing pieces before producing the bundle. Errors are blocking issues that need to be fixed first; warnings are suggestions that can be ignored if you understand the tradeoff. Each error message points to a specific GameObject so you can jump straight to it.");

            Section("Where to Get Help");
            Body("Each tab on the left documents one feature in depth. You can reopen this guide at any point from the OG Fun Monke Horror menu.");
        }

        private void DrawMapRoot()
        {
            Title("Map Root");
            Note("MapRoot is the single GameObject that represents your map. Its child GameObjects hold the level geometry, spawn points, and any optional systems like AI or triggers. The MapRoot component on this GameObject stores the map's metadata.");

            Section("Map Name");
            Body("The display name shown on the portal players walk into to load your map. Keep it under 19 characters to avoid it getting clipped in the UI.");

            Section("Portal Color");
            Body("The tint applied to the portal in the lobby. Choose whatever fits your map's atmosphere. This setting is purely cosmetic and doesn't affect lighting inside the level.");

            Section("Mods Allowed");
            Body("Controls whether player-installed mods are active while the map is loaded. Leaving this enabled is recommended for most maps. Disable it if your map relies on specific physics behaviour that mods could disrupt.");

            Section("Max Players");
            Body("Maximum number of players allowed in a public room running your map. When the game auto-joins players into your map, it uses this number as the cap. Range 1 to 50. The default is 15, which works well for most multiplayer maps. Set this lower for tight maps where many players would feel crowded, or higher for large open levels.");

            Section("Skybox");
            Body("Two modes are available:\n\n• Single Color: fills the sky with a flat color. Note that this only renders in-game, the Unity editor will continue to show the default editor skybox.\n• Material: uses a Skybox material. Choose this if you want a textured sky.");

            Section("Ambient Color");
            Body("Ambient color is the indirect light that fills the entire scene uniformly. It defaults to black.\n\nIf your map uses baked lighting, the ambient color only affects dynamic objects, the player, AI, and anything that moves. Static geometry uses the baked lightmap instead. For maps without baked lighting, ambient is the main source of fill light and should be tinted to match your scene's mood.");

            Section("Fog");
            Body("Fog gradually fades distant geometry into a chosen color, which is useful for creating a sense of depth or hiding the edges of small levels.\n\n• Start Distance: how far from the camera fog begins to appear.\n• End Distance: how far from the camera fog becomes fully opaque.\n\nThe fog color should usually match either the skybox or ambient color for the most natural result. Fog has a small per-frame cost, keep the end distance close to where you actually want the fade to stop.");
        }

        private void DrawSpawnPoints()
        {
            Title("Spawn Points");
            Note("Spawn points define where the player appears when your map loads. They live as children of MapRoot.");

            Section("Creating a Spawn Point");
            Body("Add an empty GameObject as a child of MapRoot and name it PlayerSpawn. The player's position will match the spawn point's position when the map loads, and the player's initial look direction will match its rotation.");

            Section("Multiple Spawn Points");
            Body("You can add more than one PlayerSpawn. If multiple are present, the game picks one at random each time someone enters the map. This is useful for replayability or for maps with multiple starting locations.");

            Section("Placement Tips");
            Body("Place spawn points slightly above floor geometry to avoid the player clipping into the floor on load. Make sure each spawn point has line of sight to whatever you want the player to see first, they will be facing the spawn's forward direction (the blue arrow in the Scene view).");
        }

        private void DrawCreatingMaps()
        {
            Title("Creating Your Map");
            Note("All the visible content of your map lives inside the Environment GameObject. Walls, floors, props, lights, decorations, anything the player can see or interact with.");

            Section("The Environment Object");
            Body("MapRoot/Environment is reserved for your level geometry. The exporter only packages objects that are inside this hierarchy, so anything outside of it won't appear in-game.");
            EditorGUILayout.HelpBox("If a model is showing up in the editor but missing in-game, the most common cause is that it was placed outside MapRoot/Environment. Drag it inside and re-export.", MessageType.Warning);

            Section("Unity's Built-in Meshes");
            Body("The default cube, sphere, capsule, and plane primitives are useful for blocking out a layout quickly. They are not ideal for a final map. Making a whole map with these looks bland and lacks visual interest. Treat them as placeholders and replace them with proper geometry once your layout is decided.");

            Section("Using Blender");
            Body("Blender (free at blender.org) is the most common tool for creating original 3D meshes. The general workflow is:\n\n1. Model your object in Blender.\n2. Apply transforms with Ctrl+A → All Transforms before exporting. This bakes scale and rotation into the mesh data, preventing issues in Unity.\n3. Export as an FBX file (File → Export → FBX).\n4. Drag the FBX into your Unity project and place it under MapRoot/Environment.\n\nKeep performance in mind: High triangle meshes can lag. Aim for fewer than 10,000 triangles per large structure and fewer than 2,000 per small prop. Use texture detail and lighting to add visual richness instead of polycount.");

            Section("Materials and Textures");
            Body("Use the Universal Render Pipeline (URP) Lit shader for almost everything. It supports albedo, normal, metallic, smoothness, and emission maps. If a material is pink, make sure the shader being used is the Universal Render Pipelin/Lit shader.");

            Section("Triggers");
            Body("Triggers are colliders that fire an action when the player touches them. Teleporting, opening doors, playing audio, spawning prefabs can be done with these triggers. See the Triggers tab for details.");
        }

        private void DrawTriggers()
        {
            Title("Triggers");
            Note("A trigger is a GameObject with a collider set to Is Trigger that runs a piece of behaviour when the player walks into it. The template includes five trigger types: Teleporter, ToggleOnTriggered, AudioOnTriggered, SpawnOnTriggered, and DoorTrigger.");

            Section("Common Setup");
            Body("Every trigger uses the same base setup. Before adding a trigger component, configure the GameObject like this:\n\n1. Create an empty GameObject inside MapRoot/Environment.\n2. Add a Collider component, Box, Sphere, or Capsule are recommended. A Mesh Collider can be used, but it must have Convex enabled.\n3. On the Collider, enable Is Trigger. This makes the collider fire trigger events instead of physically blocking the player.\n4. Set the GameObject's Layer to Ignore Raycast. This prevents the trigger from being interactable in unintended ways.\n\nThe Inspector for each trigger component will show red error messages if any of these requirements are missing, so you don't have to remember the list.\n\nTriggers fire on contact with the player's body or hands, not on physics objects or held items.");

            Section("Teleporter");
            Body("Moves the player to one of several preset destinations when they walk into the trigger.\n\n1. Complete the Common Setup above.\n2. Add Component → Teleporter.\n3. In the Inspector, drag one or more Transforms into the Teleport Points list.\n\nWith one point, the player always lands there. With multiple points, the game picks one at random each time — useful for spreading players across spawn locations or for randomized progression.");

            Section("ToggleOnTriggered");
            Body("Changes the active state of one or more GameObjects. The most flexible trigger type, it can drive almost any scripted moment because any GameObject can be turned on or off.\n\n1. Complete the Common Setup above.\n2. Add Component → ToggleOnTriggered.\n3. Drag the GameObjects you want to affect into Targets.\n4. Pick a Mode:\n   • Toggle: flips each target's active state.\n   • Enable: forces every target on.\n   • Disable: forces every target off.\n5. Enable One Shot if the trigger should only fire the first time.\n\nTargets can be anything: lights, props, audio, particles, other triggers, or whole sections of geometry. The classic reveal pattern is to start a target disabled and have a trigger Enable it later when the player reaches a specific point.");

            Section("AudioOnTriggered");
            Body("Plays a sound effect when the player walks into the trigger. Use it for stingers, ambient layers, dialogue lines, footsteps, or any one-off audio cue.\n\n1. Complete the Common Setup above.\n2. Add Component → AudioOnTriggered. An AudioSource is added automatically.\n3. Drag an AudioClip into the Clip field.\n4. Adjust Volume (0 to 1) and Spatial Blend (0 = stereo, 1 = full 3D positional).\n5. Enable Loop if the clip should keep playing continuously after triggering.\n6. Enable One Shot if the trigger should only fire once.\n\nFor jump-scare stingers, use spatial blend of 1 and place the trigger directly where you want the sound to come from. For ambient music or background loops, use spatial blend 0 so the audio plays evenly throughout the map.");

            Section("SpawnOnTriggered");
            Body("Instantiates a prefab when the player walks into the trigger. Useful for spawning monsters, props, particle effects, or even entire sections of geometry that shouldn't exist until the player reaches a certain point.\n\n1. Complete the Common Setup above.\n2. Add Component → SpawnOnTriggered.\n3. Drag a prefab into the Prefab field.\n4. Either tick Use Trigger Transform (spawns at this trigger's own position) or drag a separate Transform into Spawn Point (spawns there instead).\n5. Enable One Shot to prevent the spawn from happening every time the player re-enters.\n\nFor a monster that pops up behind the player, place an empty GameObject just behind their typical sight line, assign it as Spawn Point, and disable One Shot if you want it to keep respawning after death.");

            Section("DoorTrigger");
            Body("Creates an animated door that opens and closes when the player presses it. The door slides between an Open Position and a Closed Position with smooth motion and synchronized audio.\n\n1. Complete the Common Setup above. DoorTrigger specifically requires a BoxCollider (not Sphere or Capsule).\n2. Add Component → DoorTrigger. A 3D AudioSource is added automatically.\n3. Drag your door's mesh GameObject into Door Object.\n4. Set Open Position and Closed Position. These are local positions relative to the door's parent — usually you'd record the door's current position as Closed and an offset position as Open.\n5. Set Speed (units per second the door travels).\n6. Optionally assign a Button Audio Clip (plays when pressed) and a Door Audio Clip (plays when the door finishes moving).\n\nAuto Open lets the door re-open automatically after a timer if it's closed. Enable Auto Open and set Auto Open Timer in seconds. Useful for one-way doors that lock the player into a room temporarily.\n\nNetwork Mode controls multiplayer sync:\n   • Client Side: the door isn't networked. Each player sees their own door state. Use this for cosmetic doors or single-player puzzles.\n   • Server Side: the door is fully synchronized. When one player opens it, every player in the room sees it open. Use this for doors that affect everyone, like the meetup, corner, or forest doors in the base game.");

            Section("Chaining Triggers");
            Body("Triggers can drive each other. ToggleOnTriggered can Enable a DoorTrigger that didn't exist before, which can later be triggered by an AudioOnTriggered placed inside it. This is how scripted moments and progression are built — entirely through chained triggers, with no scripting required.\n\nExample sequence:\n1. Player enters Room A.\n2. ToggleOnTriggered (One Shot) enables a hidden monster prefab in Room B.\n3. AudioOnTriggered in Room A plays a footstep sound.\n4. SpawnOnTriggered behind the player drops a key prop.\n5. ToggleOnTriggered (One Shot) on the key prop disables the door blocking Room B once the player picks it up.\n\nNone of those steps require writing code.");
        }

        private void DrawVisuals()
        {
            Title("Visuals");
            Note("This section covers post-processing, lighting, and how to use them effectively on Quest. The Quest's GPU is limited, so the goal is high visual impact at low runtime cost.");

            Section("Post-Processing Volumes");
            Body("A Volume applies full-screen effects like bloom, color grading, and vignette to your map. To add one:\n\n1. Create an empty GameObject inside MapRoot/Visuals and name it Volume.\n2. Add Component → Rendering → Volume.\n3. Enable Is Global so the effects apply everywhere in your map.\n4. Click New next to Profile to create a new Volume Profile asset.\n5. Click Add Override on the profile and pick the effects you want.\n\nBloom and color grading are inexpensive and almost always look good.");

            Section("Baked Lighting (Unity)");
            Body("Baked lighting is precomputed at build time and has effectively zero runtime cost, which makes it the preferred lighting method for Quest. To set it up:\n\n1. Open Window → Rendering → Lighting.\n2. Set each Light in your scene to Baked or Mixed mode.\n3. Select your static geometry and mark it as Static under Static → Contribute GI in the Inspector.\n4. Click Generate Lighting.\n\nThe lightmap will be computed and saved into the scene. Re-bake any time you change a Baked light or modify static geometry.");

            Section("Baked Lighting (Bakery)");
            Body("Bakery is a third-party lightmapper available on the Unity Asset Store. It produces noticeably higher quality results than Unity's built-in lightmapper and is significantly faster on supported hardware.\n\nTo use Bakery in this template, replace standard Unity Lights with BakeryPointLight, BakeryDirectLight, or BakeryAreaLight components, then bake from the Bakery window.");
            EditorGUILayout.HelpBox("Bakery only supports modern Nvidia GPUs (RTX series recommended). It does not run on AMD or Apple Silicon.", MessageType.Warning);

            Section("Realtime Lights");
            Body("Realtime lights are calculated every frame and can be moved or changed at runtime. They are useful for dynamic effects but expensive on Quest, so use them sparingly.\n\n• Point Light: emits in all directions from a single point. Cheap when shadows are off.\n• Spot Light: emits in a cone shape. Good for flashlights or focused beams.\n• Directional Light: emulates sunlight. Keep it set to Baked or Mixed, realtime directional lights with shadows are expensive.\n• Area Light: rectangular emitter. Baked-only.\n\nLimit the number of shadow-casting realtime lights visible at any one time to 2 or 3. Stencil shadows from realtime lights are a common source of frame drops on Quest 2.");
        }

        private void DrawAI()
        {
            Title("AI");
            Note("This template supports two AI types built on Unity's NavMesh system. Neutral AI wanders between waypoints and ignores the player. Monster AI also wanders, but actively detects the player within a configurable cone of vision and chases them.");

            Section("Step 1, Bake the NavMesh");
            Body("Unity uses a NavMesh, a precomputed map of walkable surfaces to make AI movement. The template includes a prebuilt NavMeshSurface GameObject located at MapRoot/AI/NavMeshSurface.\n\nSelect this GameObject and click Bake in the NavMeshSurface component. Unity scans your geometry and produces a blue overlay showing walkable areas. Re-bake any time you modify the geometry your AI needs to traverse.");

            Section("Step 2, Add a NavMeshAgent");
            Body("Each AI GameObject needs a NavMeshAgent component, which connects it to the NavMesh you just baked.\n\nAdd Component → AI → NavMesh Agent on your AI GameObject. Set Radius and Height to roughly match the dimensions of your AI's collider, too small and the AI will clip through obstacles, too large and it won't fit through doorways. Leave Speed at its default. MapAI overrides it at runtime.");

            Section("Step 3, Add MapAI");
            Body("MapAI is the script that controls the AI's behaviour. Add Component → OGFunMonkeHorror → MapAI and choose either Neutral or Monster from the AI Type dropdown. The Inspector automatically hides fields that don't apply to the chosen type.");

            Section("Step 4, Set Up Waypoints");
            Body("Both AI types patrol between waypoints when they aren't actively pursuing the player. To create them:\n\n1. Create empty GameObjects as children of MapRoot/Waypoints.\n2. Position each waypoint on a walkable area of the NavMesh, anywhere the blue overlay covers.\n3. Drag each waypoint into the Waypoints array on your MapAI component.\n\nUse at least 4 waypoints per AI to avoid repetitive patterns. Every waypoint must be reachable on the NavMesh; an unreachable waypoint will cause the AI to stall or behave erratically.");

            Section("Required Layer");
            Body("The root GameObject of every AI (both Neutral and Monster) must have its Layer set to Ignore Raycast. This prevents the AI's own collider from interfering with the line-of-sight check that Monster AI uses to detect the player. The exporter will block your map from building if this is misconfigured.");

            Section("Monster Detection Settings");
            Body("Monster AI uses a vision cone combined with a raycast to detect the player.\n\n• Chase Distance: maximum distance at which the monster can detect the player, in metres.\n• Field of View: width of the vision cone in degrees. 180 is a reasonable default. 360 makes the monster omniscient and is rarely fun.\n• Chase Audio: an AudioSource that plays while the monster is actively pursuing. Make sure Play On Awake is disabled, MapAI starts and stops the audio itself.\n\nWhen the player enters the vision cone, the monster raycasts toward them. If the ray reaches the player without being blocked by geometry, the chase begins.");

            Section("Monster Collider");
            Body("Monsters need a Collider with Is Trigger enabled so the game can detect when the player makes contact and trigger the jumpscare.\n\nDo not use a Mesh Collider on a Monster. Use Box, Sphere, or Capsule. Mesh Colliders as triggers behave unreliably and are not supported.");

            Section("Jumpscare");
            Body("Add a Jumpscare component to the same GameObject as MapAI to define what happens when the monster catches the player.\n\nThe component needs two things:\n• Jumpscare Prefab: a prefab spawned in front of the player's camera for 2 seconds when they are caught.\n• Respawn Points: a list of Transforms. The player is teleported to a random one after the jumpscare ends.\n\nA jumpscare prefab typically contains:\n• An AudioSource for the scare sound.\n• A black inside-out box that completely surrounds the player so they can't see the scene during the scare.\n• Your monster mesh inside the box, facing the player. Add an Animator if you want the monster to play an animation during the scare.");

            Section("Scene View Gizmos");
            Body("Selecting a Monster AI in the Scene view shows its detection ranges as colored gizmos:\n• Red wireframe sphere is Chase Distance.\n• Yellow cone is Field of View.\n\nUse these to confirm the monster's detection covers the area you intend before testing in-game.");
        }

        private void DrawExporting()
        {
            Title("Exporting");
            Note("Exporting packages your scene into an AssetBundle for the main game to load at runtime. The export is produced as a single .zip file that you upload to your mod.io game page.");

            Section("Export Steps");
            Body("1. Open the exporter from OG Fun Monke Horror → Export.\n2. Assign your MapRoot GameObject to the MapRoot field.\n3. Click Browse and choose an output folder for the exported .zip.\n4. Review the validation panel. Fix any red errors before continuing, you cannot export until they're resolved.\n5. Click Export Map. The build process takes anywhere from a few seconds to a few minutes depending on map size.\n6. The .zip is saved to your chosen folder. Upload it to your mod.io game page for the map to become available in-game.");

            Section("After Exporting");
            Body("The exporter automatically applies a few project setting tweaks behind the scenes to keep your map compatible with the main game's rendering — normal map encoding and color space are aligned during export. You don't need to manage these yourself, but if you ever notice your textures looking off after a re-import, those are the settings that were adjusted.");
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
