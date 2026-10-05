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
            BuildingYourMap,
            Triggers,
            Visuals,
            AI,
            Scripting,
            Exporting,
        }

        private Tab _tab = Tab.GettingStarted;
        private Vector2 _sidebarScroll;
        private Vector2 _contentScroll;

        private static GUIStyle _codeStyle;
        private static GUIStyle _apiNameStyle;
        private static GUIStyle _apiTextStyle;
        private static Font _monoFont;

        private static readonly string[] TabLabels =
        {
            "Getting Started",
            "Map Root",
            "Spawn Points",
            "Building Your Map",
            "Triggers",
            "Visuals",
            "AI",
            "Scripting",
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
                case Tab.BuildingYourMap: DrawBuildingYourMap(); break;
                case Tab.Triggers: DrawTriggers(); break;
                case Tab.Visuals: DrawVisuals(); break;
                case Tab.AI: DrawAI(); break;
                case Tab.Scripting: DrawScripting(); break;
                case Tab.Exporting: DrawExporting(); break;
            }

            EditorGUILayout.Space(16);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawGettingStarted()
        {
            Title("Getting Started");
            Note("This template has everything you need to make a custom map for OG Fun Monke Horror and upload it to mod.io. The tabs on the left are in the order you'll usually need them.");

            Section("How a Map Is Put Together");
            Body("Every map is a Unity scene with one main object called MapRoot. Everything that belongs to your map goes inside MapRoot: the level itself, spawn points, lights, AI and triggers. When you export, the scene is packed into a file that the game downloads and loads.");

            Section("The Basic Steps");
            Body("1. Open the Template1 scene in Assets/Scenes. MapRoot is already set up in it.\n" +
                 "2. Select MapRoot and set the map name, portal color and lighting in the Inspector.\n" +
                 "3. Build your level inside MapRoot/Environment.\n" +
                 "4. Add triggers, AI and effects if you want them. Each one has its own tab here.\n" +
                 "5. Bake your lighting from Window → Rendering → Lighting.\n" +
                 "6. Export from OG Fun Monke Horror → Export and upload the .zip to mod.io.");

            Section("Checks Before You Export");
            Body("The exporter checks your scene before it builds anything. Red errors must be fixed before you can export. Yellow warnings are worth reading, but you can still export with them. Every message names the object it's talking about, so you know where to look.");

            Section("Opening This Guide Again");
            Body("You can open this guide any time from OG Fun Monke Horror → Guide.");
        }

        private void DrawMapRoot()
        {
            Title("Map Root");
            Note("MapRoot is the main object of your map. Its settings in the Inspector control how the map appears in the lobby and how it looks once it loads.");

            Section("Map Name");
            Body("The name shown on the portal in the lobby. It can be up to 19 characters long.");

            Section("Portal Color");
            Body("The color of the portal players walk into to load your map. It only changes the portal, not anything inside your map.");

            Section("Mods Allowed");
            Body("Whether players can use their installed mods on your map. Most maps can leave this on. Turn it off if mods would break your map, for example if it depends on how fast players move or how high they jump.");

            Section("Max Players");
            Body("The most players that can be in a public room on your map, from 1 to 50. The default is 15. Use a lower number for small, tight maps and a higher one for big open maps.");

            Section("Skybox");
            Body("• Single Color fills the sky with one flat color. You'll only see it in the game. The Unity editor keeps showing its normal sky.\n" +
                 "• Material uses a skybox material, for a sky with a texture.");

            Section("Ambient Color");
            Body("Ambient light is soft light that reaches every part of the map evenly. It starts out black.\n\n" +
                 "If you bake your lighting, ambient color only affects things that move, like players and AI. The baked lighting handles everything else. If you don't bake, ambient color is most of your lighting, so pick one that fits the mood of your map.");

            Section("Fog");
            Body("Fog fades distant things into a single color. It adds depth and can hide the edges of a small map.\n\n" +
                 "• Start Distance is where the fog begins.\n" +
                 "• End Distance is where everything is completely covered.\n\n" +
                 "Fog usually looks best when its color matches your sky or ambient color. It has a small performance cost on Quest.");
        }

        private void DrawSpawnPoints()
        {
            Title("Spawn Points");
            Note("Spawn points are where players appear when they enter your map.");

            Section("Adding a Spawn Point");
            Body("Create an empty object inside MapRoot/PlayerSpawns. Players appear at its position and face the way its blue arrow points.");

            Section("More Than One Spawn Point");
            Body("If you add several, the game picks one at random each time someone enters your map.");

            Section("Tips");
            Body("Raise spawn points slightly above the floor so players don't spawn inside it. Point each one at whatever you want players to see first.");
        }

        private void DrawBuildingYourMap()
        {
            Title("Building Your Map");
            Note("Everything players can see and touch goes inside MapRoot/Environment: walls, floors, props, lights and decorations.");

            Section("Keep Everything Inside Environment");
            Body("The game finds and places your map through MapRoot, so anything left outside MapRoot/Environment can end up missing or in the wrong place.");
            EditorGUILayout.HelpBox("If something shows up in Unity but not in the game, check that it's inside MapRoot/Environment, then export again.", MessageType.Warning);

            Section("Unity's Basic Shapes");
            Body("Cubes, spheres and planes are great for blocking out a layout quickly, but a whole map made of them looks plain. Use them to test your layout, then replace them with real models.");

            Section("Making Models in Blender");
            Body("Blender is free at blender.org and is the most common way to make your own models.\n\n" +
                 "1. Model your object.\n" +
                 "2. Select it and press Ctrl+A → All Transforms so its size and rotation come out right in Unity.\n" +
                 "3. Export it with File → Export → FBX.\n" +
                 "4. Drag the FBX into Unity and place it inside MapRoot/Environment.\n\n" +
                 "Quest doesn't have much power, so keep models simple. Aim for under 10,000 triangles for large structures and under 2,000 for small props. Textures and lighting add detail far more cheaply than extra triangles.");

            Section("Materials");
            Body("Use the Universal Render Pipeline/Lit shader for almost everything. If a material shows up pink, it's using a shader the game can't draw. Switch it to Universal Render Pipeline/Lit.");

            Section("Hit Sounds");
            Body("When a player hits a surface, the game plays a sound based on its material. Materials with the same name as one of the game's own materials, like the Brick, grid and Glow materials in this template, use the game's sound automatically. Any other material is silent unless you give it sounds.\n\n" +
                 "To give your materials their own sounds:\n" +
                 "1. Select MapRoot and choose Add Component → OG Fun Monke Horror → Map Hitsounds.\n" +
                 "2. Add an entry for each group of materials that should sound the same.\n" +
                 "3. Drag those materials into Materials and the sound files into Sounds.\n\n" +
                 "Each hit plays one of the entry's sounds at random. Use a single Map Hitsounds component per map.");

            Section("Triggers");
            Body("Triggers make things happen when a player touches them, like teleporting, opening doors or playing sounds. See the Triggers tab.");
        }

        private void DrawTriggers()
        {
            Title("Triggers");
            Note("A trigger is an invisible area that does something when a player's body or hands touch it. The template has five: Teleporter, ToggleOnTriggered, AudioOnTriggered, SpawnOnTriggered and DoorTrigger.");

            Section("Setting Up Any Trigger");
            Body("Every trigger starts the same way:\n\n" +
                 "1. Create an empty object inside MapRoot/Environment.\n" +
                 "2. Add a Box, Sphere or Capsule Collider. A Mesh Collider also works, but only with Convex turned on.\n" +
                 "3. Turn on Is Trigger on the collider, so players pass through it instead of bumping into it.\n" +
                 "4. Set the object's Layer to Ignore Raycast.\n\n" +
                 "If you miss a step, the trigger's Inspector shows a red message telling you what to fix.\n\n" +
                 "Triggers react to players' bodies and hands. They don't react to physics objects or to items players are holding.");

            Section("Teleporter");
            Body("Sends the player somewhere else.\n\n" +
                 "1. Set up the trigger as described above.\n" +
                 "2. Add Component → Teleporter.\n" +
                 "3. Drag one or more objects into Teleport Points.\n\n" +
                 "With one point, players always land there. With several, the game picks one at random each time.");

            Section("ToggleOnTriggered");
            Body("Turns objects on or off. Almost anything can be turned on or off, so this is the most useful trigger for scripted moments.\n\n" +
                 "1. Set up the trigger as described above.\n" +
                 "2. Add Component → ToggleOnTriggered.\n" +
                 "3. Drag the objects you want to change into Targets.\n" +
                 "4. Choose a Mode:\n" +
                 "   • Toggle switches each target to the opposite state.\n" +
                 "   • Enable turns every target on.\n" +
                 "   • Disable turns every target off.\n" +
                 "5. Turn on One Shot if it should only work the first time.\n\n" +
                 "A common trick is to start something turned off, like a monster or a hidden room, and turn it on when the player reaches a certain spot.");

            Section("AudioOnTriggered");
            Body("Plays a sound when a player walks in. Good for jump scare stings, music and voice lines.\n\n" +
                 "1. Set up the trigger as described above.\n" +
                 "2. Add Component → AudioOnTriggered. An AudioSource is added for you.\n" +
                 "3. Drag a sound into Clip.\n" +
                 "4. Set Volume (0 to 1) and Spatial Blend. At 0 the sound plays at the same volume everywhere. At 1 it comes from the trigger's position.\n" +
                 "5. Turn on Loop to keep it playing after it starts.\n" +
                 "6. Turn on One Shot if it should only play once.\n\n" +
                 "For a scare, use a Spatial Blend of 1 and put the trigger where the sound should come from. For background music, use 0.");

            Section("SpawnOnTriggered");
            Body("Creates a copy of a prefab when a player walks in, such as a monster, a prop or an effect.\n\n" +
                 "1. Set up the trigger as described above.\n" +
                 "2. Add Component → SpawnOnTriggered.\n" +
                 "3. Drag a prefab into Prefab.\n" +
                 "4. Turn on Use Trigger Transform to spawn it at the trigger, or drag an object into Spawn Point to spawn it there instead.\n" +
                 "5. Turn on One Shot so it doesn't spawn again every time someone walks back in.");

            Section("DoorTrigger");
            Body("A door that slides open and shut when a player presses it.\n\n" +
                 "1. Set up the trigger as described above. DoorTrigger needs a Box Collider.\n" +
                 "2. Add Component → DoorTrigger. An AudioSource is added for you.\n" +
                 "3. Drag the door model into Door Object.\n" +
                 "4. Set Closed Position and Open Position. They're positions relative to the door's parent. The easiest way is to copy the door's current position into Closed Position, then work out where it should slide to for Open Position.\n" +
                 "5. Set Speed to how fast the door moves.\n" +
                 "6. For sounds, add a Button Audio Clip (plays when pressed) and a Door Audio Clip (plays when the door stops).\n\n" +
                 "Turn on Auto Open to make a closed door open again by itself after Auto Open Timer seconds.\n\n" +
                 "Network Mode decides whether other players see the door move:\n" +
                 "   • Client Side: each player has their own copy of the door. Good for puzzles and decoration.\n" +
                 "   • Server Side: everyone in the room sees the same door. When one player opens it, it opens for everyone.");

            Section("Combining Triggers");
            Body("Triggers can set each other up, so you can build whole scripted sequences without writing any code. For example:\n\n" +
                 "1. A player walks into Room A.\n" +
                 "2. A ToggleOnTriggered with One Shot turns on a hidden monster in Room B.\n" +
                 "3. An AudioOnTriggered in Room A plays footsteps.\n" +
                 "4. A SpawnOnTriggered drops a key behind the player.\n" +
                 "5. When the player walks over the key, a ToggleOnTriggered with One Shot turns off the door blocking Room B.\n\n" +
                 "For anything more complex, use a Map Script. See the Scripting tab.");
        }

        private void DrawVisuals()
        {
            Title("Visuals");
            Note("Lighting and post-processing make the biggest difference to how your map looks. Quest has a limited graphics chip, so the aim is to look good while costing as little as possible.");

            Section("Post-Processing");
            Body("Post-processing adds effects to the whole screen, like bloom, color grading and vignette.\n\n" +
                 "1. Create an empty object inside MapRoot/Visuals and name it Volume. It must be called exactly Volume.\n" +
                 "2. Add Component → Rendering → Volume.\n" +
                 "3. Turn on Is Global so the effects apply everywhere in your map.\n" +
                 "4. Click New next to Profile.\n" +
                 "5. Click Add Override and pick the effects you want.\n\n" +
                 "Bloom and color grading are cheap and look good on almost any map. Avoid Depth of Field, which is slow on Quest.");

            Section("Baked Lighting with Unity");
            Body("Baked lighting is worked out ahead of time, so it costs almost nothing while playing. It's the best choice for Quest.\n\n" +
                 "1. Open Window → Rendering → Lighting.\n" +
                 "2. Set your lights to Baked or Mixed.\n" +
                 "3. Select your level geometry and turn on Static → Contribute GI in the Inspector.\n" +
                 "4. Click Generate Lighting.\n\n" +
                 "Bake again whenever you move a baked light or change static geometry.\n\n" +
                 "Keep baked lights inside MapRoot/Visuals/Lighting/BakedLights and realtime lights inside MapRoot/Visuals/Lighting/RealtimeLights. The exporter checks this.");

            Section("Baked Lighting with Bakery");
            Body("Bakery is a paid lightmapper from the Unity Asset Store. It looks better than Unity's built-in baking and is usually much faster. Use Bakery's own light components instead of Unity's lights, then bake from the Bakery window.");
            EditorGUILayout.HelpBox("Bakery only runs on NVIDIA graphics cards, and an RTX card is recommended. It doesn't work on AMD or Apple Silicon.", MessageType.Warning);

            Section("Realtime Lights");
            Body("Realtime lights are calculated every frame, so they can move and change while playing. They're expensive on Quest, so only use them when you need to.\n\n" +
                 "• Point Light shines in every direction from one point. It's cheap without shadows.\n" +
                 "• Spot Light shines in a cone, like a flashlight.\n" +
                 "• Directional Light works like the sun. Keep it set to Baked or Mixed, because realtime sun shadows are expensive.\n" +
                 "• Area Light is a glowing rectangle and only works when baked.\n\n" +
                 "Try not to have more than 2 or 3 shadow-casting realtime lights on screen at once.");
        }

        private void DrawAI()
        {
            Title("AI");
            Note("There are two kinds of AI. Neutral AI walks between waypoints and ignores players. Monster AI also walks between waypoints, but it chases any player it can see.");

            Section("Step 1: Bake the NavMesh");
            Body("AI finds its way around using a NavMesh, which is a map of the places it can walk. The template already has one at MapRoot/AI/NavMeshSurface. Select it and click Bake, and the walkable areas show up in blue. Bake again whenever you change the parts of the level your AI walks on.");

            Section("Step 2: Add a NavMesh Agent");
            Body("Select your AI object and choose Add Component → AI → NavMesh Agent. Set Radius and Height to roughly match the AI's size. If they're too small, the AI clips through walls. If they're too big, it won't fit through doors. Leave Speed as it is, because MapAI sets it while playing.");

            Section("Step 3: Add MapAI");
            Body("Click Add Component, search for MapAI and add it. Set AI Type to Neutral or Monster. The Inspector hides the settings that don't apply to the type you picked.");

            Section("Step 4: Add Waypoints");
            Body("Both kinds of AI walk between waypoints when they aren't chasing anyone.\n\n" +
                 "1. Create empty objects inside MapRoot/Waypoints.\n" +
                 "2. Place each one somewhere that's blue on the NavMesh.\n" +
                 "3. Drag them into the Waypoints list on MapAI.\n\n" +
                 "Use at least 4 waypoints so the AI doesn't walk the same short loop. Every waypoint must be on the NavMesh, or the AI can get stuck.");

            Section("Layer");
            Body("Set the AI object's Layer to Ignore Raycast, for both kinds of AI. Otherwise the AI's own collider can block a monster's view of players. The exporter won't export until this is set.");

            Section("How Monsters See Players");
            Body("• Chase Distance is how far away, in meters, the monster can spot a player.\n" +
                 "• Field of View is how wide its view is, in degrees. 180 is a good place to start. 360 lets it see behind itself, which usually isn't much fun.\n" +
                 "• Chase Audio is an AudioSource that plays while it's chasing. Turn off Play On Awake on it, because MapAI starts and stops it.\n\n" +
                 "When a player is inside the monster's view, it checks whether a wall is in the way. If nothing blocks it, the chase starts.");

            Section("Monster Collider");
            Body("A monster needs a Box, Sphere or Capsule Collider with Is Trigger turned on, so the game knows when it touches a player. Don't use a Mesh Collider on a monster, because it doesn't work reliably as a trigger.");

            Section("Jumpscare");
            Body("Add a Jumpscare component to the monster to decide what happens when it catches someone.\n\n" +
                 "• Jumpscare Prefab is shown in front of the player for 2 seconds.\n" +
                 "• Respawn Points are where the player is sent afterwards. One is picked at random.\n\n" +
                 "A jumpscare prefab usually has a scare sound (an AudioSource), a black box around the player so they only see the scare, and the monster model inside the box facing them. Add an Animator if the monster should move during the scare.");

            Section("Seeing a Monster's Range");
            Body("Select a monster in the Scene view to see how far it can see. The red sphere is its Chase Distance and the yellow cone is its Field of View. Check these before testing in the game.");
        }

        private void DrawScripting()
        {
            Title("Scripting");
            Note("Map Scripts let you do things triggers can't. They're written in Lua 5.2, and most of the names match Unity, so they'll look familiar if you've used Unity before. Call methods with a colon, like door:SetActive(false), and read properties with a dot, like door.transform.");

            Section("Creating a Script");
            Body("Right-click in the Project window and choose Create → OG Fun Monke Horror → Map Script (Lua). Add a Map Script component to an object and drag the .lua file into its Lua Script field. Click Edit Script to open it. If the script has a mistake, the error shows up in the Inspector straight away.");

            Section("Events");
            Body("Write any of these functions in your script and the game calls them for you:");
            Api("onStart()", "Runs once, when the map loads.");
            Api("tick()", "Runs every frame. Multiply by Time.deltaTime for smooth movement.");
            Api("onTriggerEnter(player)", "Runs when a player enters this object's trigger.");
            Api("onTriggerExit(player)", "Runs when a player leaves this object's trigger.");

            Section("Inspector Fields");
            Body("Put a fields table at the top of your script to get slots you can fill in from the Inspector, like public variables in Unity. Write a type name for an object slot, or a value for a number, checkbox or text field that starts with that value.");
            Code("fields = {\n    door   = GameObject,\n    clip   = AudioClip,\n    speed  = 3.0,\n    active = true,\n    label  = \"hello\",\n}");
            Body("Object slots can be GameObject, Transform, TMP_Text, AudioSource, AudioClip, Material, Light, Animator, Rigidbody, Renderer or ParticleSystem. Each field becomes a variable with the same name in your script, already set to whatever you picked in the Inspector.");

            Section("GameObject");
            Api("GameObject.Find(\"Name\")", "Finds an object in your map by name. Gives nil if there isn't one.");
            Api("go.name", "The object's name.");
            Api("go.activeSelf", "Whether the object is turned on.");
            Api("go:SetActive(true)", "Turns the object on or off.");
            Api("go.transform", "The object's Transform.");
            Api("go:GetComponent(\"AudioSource\")", "A component on the object, or nil if it doesn't have one.");

            Section("Transform");
            Api("t.position", "Position in the world, as a Vector3.");
            Api("t.localPosition", "Position relative to its parent.");
            Api("t.rotation", "Rotation in the world, as a Quaternion.");
            Api("t.localScale", "Size.");
            Api("t:Translate(x, y, z)", "Moves it.");
            Api("t:Rotate(x, y, z)", "Rotates it by these angles in degrees.");

            Section("Components");
            Api("AudioSource", ":Play()  :Stop()  :PlayOneShot(clip)  .volume");
            Api("Animator", ":SetTrigger(\"name\")  :SetBool(\"name\", true)");
            Api("Light", ".enabled  .intensity  .color");
            Api("Rigidbody", ":AddForce(x, y, z)  .velocity  .useGravity");
            Api("Renderer", ".enabled");
            Api("TMP_Text", ".text  .color  .fontSize");
            Api("ParticleSystem", ":Play()  :Stop()  :Emit(count)");

            Section("Values");
            Api("Vector3(x, y, z)", "A position or direction. Read v.x, v.y and v.z. You can add vectors together and multiply them by a number.");
            Api("Quaternion.Euler(x, y, z)", "A rotation made from angles in degrees.");
            Api("Quaternion.identity", "No rotation.");
            Api("Color(r, g, b)", "A color. Each value goes from 0 to 1.");

            Section("Vector Math");
            Api("a:Distance(b)", "The distance between two points.");
            Api("a:Lerp(b, t)", "A point between a and b. t goes from 0 (at a) to 1 (at b).");
            Api("a:Dot(b)  a:Cross(b)", "Dot product and cross product.");
            Api("v.magnitude", "The vector's length.");
            Api("v.normalized", "The same direction with a length of 1.");
            Api("Quaternion.LookRotation(dir)", "A rotation that faces dir.");
            Api("Quaternion.Slerp(a, b, t)", "A smooth blend between two rotations.");

            Section("Raycasts");
            Body("Physics.Raycast shoots an invisible line and tells you what it hit, or gives nil if it didn't hit anything.");
            Code("local hit = Physics.Raycast(origin, direction, distance)\nif hit then\n    Debug.Log(hit.gameObject.name)\nend");
            Body("A hit has gameObject (what was hit), point (where it was hit), normal (which way the surface faces) and distance (how far away it was).");

            Section("Spawning Objects");
            Body("Object.Instantiate makes a copy of a prefab you put in a GameObject field. Call Destroy on anything you spawn once you're done with it. Each script can only spawn a limited number of objects.");
            Code("fields = { bullet = GameObject }\n\nfunction onStart()\n    local b = Object.Instantiate(bullet, LocalPlayer.mainCamera.position, Quaternion.identity)\n    b:GetComponent(\"Rigidbody\"):AddForce(0, 0, 20)\n    after(5, function() b:Destroy() end)\nend");

            Section("Environment");
            Body("Change the map's lighting and fog while it's running. These changes only affect each player's own view, so nothing needs to be synced between players.");
            Api("Environment.ambientColor", "The soft light that reaches everything.");
            Api("Environment.fog", "Turns fog on or off.");
            Api("Environment.fogColor  .fogStart  .fogEnd", "The fog's color and distances.");
            Api("Environment.skyColor", "A flat sky color.");
            Api("Environment.skybox", "A skybox material, for example from a Material field.");

            Section("Utilities");
            Api("Mathf.Sin  Mathf.Lerp  Mathf.Clamp ...", "The usual math functions, plus Mathf.PI.");
            Api("Time.deltaTime", "Seconds since the last frame.");
            Api("Time.time", "The current time in seconds.");
            Api("Random.Range(a, b)", "A random number between a and b.");
            Api("Debug.Log(\"message\")", "Prints a message while you're testing.");
            Api("after(seconds, function() ... end)", "Runs something after a delay.");
            Api("state", "A table shared by every script in your map. Use it to pass information between scripts.");

            Section("The Local Player");
            Body("These only affect the player running the script, so nothing needs to be synced between players.");
            Api("LocalPlayer.mainCamera", "The player's head. Read .position, .rotation and .forward.");
            Api("LocalPlayer.leftHand  LocalPlayer.rightHand", "The player's hands. Read .position and .rotation.");
            Api("LocalPlayer.velocity", "How fast and in which direction the player is moving. You can read and change it.");
            Api("LocalPlayer:AddForce(x, y, z)", "Pushes the player.");
            Api("LocalPlayer.jumpMultiplier", "How strong the player's jumps are. You can read and change it.");
            Api("LocalPlayer.maxJumpSpeed", "The fastest a jump can launch the player. You can read and change it.");

            Section("Controller Input");
            Body("These update every frame. Read them inside tick().");
            Api("PlayerInput.leftXAxis  leftYAxis  rightXAxis  rightYAxis", "The thumbsticks, from -1 to 1.");
            Api("PlayerInput.leftTrigger  rightTrigger", "The triggers, from 0 to 1.");
            Api("PlayerInput.leftGrip  rightGrip", "The grip buttons, from 0 to 1.");
            Api("PlayerInput.leftPrimaryButton  rightPrimaryButton", "True while the button is held.");
            Api("PlayerInput.leftSecondaryButton  rightSecondaryButton", "True while the button is held.");

            Section("Vibration");
            Api("startVibration(leftHand, strength, duration)", "Vibrates a controller. Use true for the left hand and false for the right. Strength goes from 0 to 1, and duration is in seconds.");
            Code("startVibration(true, 0.8, 0.5)");

            Section("The Player in Trigger Events");
            Body("The player passed to onTriggerEnter and onTriggerExit is whoever touched the trigger. Read player.position to find out where they are.");

            Section("Example");
            Body("This opens a door two seconds after three players have stepped on a pressure plate. Put the script on the plate's trigger and drag the door into the door field.");
            Code("fields = { door = GameObject }\n\nfunction onTriggerEnter(player)\n    state.count = (state.count or 0) + 1\n    if state.count >= 3 then\n        after(2.0, function() door:SetActive(false) end)\n    end\nend");
        }

        private void DrawExporting()
        {
            Title("Exporting");
            Note("Exporting turns your map into a single .zip file. You upload that file to mod.io, and the game downloads it from there.");

            Section("How to Export");
            Body("1. Open OG Fun Monke Horror → Export.\n" +
                 "2. Click Browse and choose a folder to save the .zip in.\n" +
                 "3. Drag your MapRoot into the MapRoot field.\n" +
                 "4. Fix any red errors in the Validation list. The Export button stays greyed out until they're all gone.\n" +
                 "5. Click Export. Small maps take a few seconds, and large ones can take a few minutes.\n" +
                 "6. Upload the .zip to the OG Fun Monke Horror page on mod.io.");

            Section("Exporting Several Maps at Once");
            Body("Switch to Multiple Export at the top of the export window. Click Add Scene for each map and drag its scene into the new row. Every scene needs a MapRoot. Click Export All, and each map is saved as its own .zip in the output folder. The log underneath shows how each one went.");
        }

        private static Font MonoFont
        {
            get
            {
                if (_monoFont == null)
                    _monoFont = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Menlo", "Courier New" }, 12);
                return _monoFont;
            }
        }

        private static GUIStyle CodeStyle
        {
            get
            {
                if (_codeStyle != null) return _codeStyle;
                _codeStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = false,
                    richText = false,
                    font = MonoFont,
                    padding = new RectOffset(8, 8, 6, 6),
                };
                return _codeStyle;
            }
        }

        private static GUIStyle ApiNameStyle
        {
            get
            {
                if (_apiNameStyle != null) return _apiNameStyle;
                _apiNameStyle = new GUIStyle(EditorStyles.label)
                {
                    font = MonoFont,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                };
                return _apiNameStyle;
            }
        }

        private static GUIStyle ApiTextStyle
        {
            get
            {
                if (_apiTextStyle != null) return _apiTextStyle;
                _apiTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    padding = new RectOffset(16, 2, 0, 4),
                };
                return _apiTextStyle;
            }
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

        private void Api(string name, string description)
        {
            EditorGUILayout.LabelField(name, ApiNameStyle);
            EditorGUILayout.LabelField(description, ApiTextStyle);
        }

        private void Code(string code)
        {
            float height = CodeStyle.CalcHeight(new GUIContent(code), 1000f);
            EditorGUILayout.SelectableLabel(code, CodeStyle, GUILayout.Height(height));
            EditorGUILayout.Space(2);
        }
    }
}
#endif
