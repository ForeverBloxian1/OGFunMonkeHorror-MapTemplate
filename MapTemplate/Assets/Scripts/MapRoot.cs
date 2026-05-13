using UnityEngine;

public class MapRoot : MonoBehaviour
{
    private const int MapNameMaxLength = 19;

    [Header("Map Info")]
    [SerializeField] private string _mapName = "My Map";
    [SerializeField] private Color _portalColor = Color.cyan;
    [SerializeField] private bool _modsAllowed = true;
    [SerializeField, Range(1, 50)] private int _maxPlayers = 15;

    [Header("Lighting")]
    [SerializeField] private SkyboxMode _skyboxMode = SkyboxMode.SingleColor;
    [SerializeField] private Color _skyboxColor = Color.black;
    [SerializeField] private Material _skyboxMaterial;
    [SerializeField] private Color _ambientColor = Color.black;
    [SerializeField] private bool _fogEnabled = false;
    [SerializeField] private Color _fogColor = Color.black;
    [SerializeField] private float _fogStart = 0f;
    [SerializeField] private float _fogEnd = 300f;

    public enum SkyboxMode { SingleColor, Material }

    public string MapName => _mapName;
    public Color PortalColor => _portalColor;
    public bool ModsAllowed => _modsAllowed;
    public int MaxPlayers => _maxPlayers;
    public SkyboxMode SkyboxModeValue => _skyboxMode;
    public Material SkyboxMaterial => _skyboxMaterial;
    public Color SkyboxColor => _skyboxColor;
    public Color AmbientColor => _ambientColor;
    public bool FogEnabled => _fogEnabled;
    public Color FogColor => _fogColor;
    public float FogStart => _fogStart;
    public float FogEnd => _fogEnd;

    private void OnValidate()
    {
        if (_mapName != null && _mapName.Length > MapNameMaxLength)
            _mapName = _mapName.Substring(0, MapNameMaxLength);

        ApplyLighting();
    }

    private void OnEnable() => ApplyLighting();

    public void ApplyLighting()
    {
        if (_skyboxMode == SkyboxMode.SingleColor)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = _skyboxColor;
        }
        else
        {
            RenderSettings.skybox = _skyboxMaterial;
        }

        RenderSettings.ambientLight = _ambientColor;
        RenderSettings.fog = _fogEnabled;

        if (_fogEnabled)
        {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogStartDistance = _fogStart;
            RenderSettings.fogEndDistance = _fogEnd;
        }
    }
}

#if UNITY_EDITOR
namespace OGFunMonkeHorror.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(MapRoot))]
    public class MapRootEditor : Editor
    {
        private const int MaxLength = 19;

        private SerializedProperty _mapNameProp;
        private SerializedProperty _portalColorProp;
        private SerializedProperty _modsAllowedProp;
        private SerializedProperty _maxPlayersProp;
        private SerializedProperty _skyboxModeProp;
        private SerializedProperty _skyboxColorProp;
        private SerializedProperty _skyboxMaterialProp;
        private SerializedProperty _ambientColorProp;
        private SerializedProperty _fogEnabledProp;
        private SerializedProperty _fogColorProp;
        private SerializedProperty _fogStartProp;
        private SerializedProperty _fogEndProp;

        private int _tab = 0;
        private readonly string[] _tabs = { "Map Info", "Lighting" };

        private GUIStyle _tooltipStyle;

        private void OnEnable()
        {
            _mapNameProp = serializedObject.FindProperty("_mapName");
            _portalColorProp = serializedObject.FindProperty("_portalColor");
            _modsAllowedProp = serializedObject.FindProperty("_modsAllowed");
            _maxPlayersProp = serializedObject.FindProperty("_maxPlayers");
            _skyboxModeProp = serializedObject.FindProperty("_skyboxMode");
            _skyboxColorProp = serializedObject.FindProperty("_skyboxColor");
            _skyboxMaterialProp = serializedObject.FindProperty("_skyboxMaterial");
            _ambientColorProp = serializedObject.FindProperty("_ambientColor");
            _fogEnabledProp = serializedObject.FindProperty("_fogEnabled");
            _fogColorProp = serializedObject.FindProperty("_fogColor");
            _fogStartProp = serializedObject.FindProperty("_fogStart");
            _fogEndProp = serializedObject.FindProperty("_fogEnd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _tooltipStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };

            _tab = GUILayout.Toolbar(_tab, _tabs);
            EditorGUILayout.Space(6);

            if (_tab == 0)
                DrawMapInfo();
            else
                DrawLighting();

            if (serializedObject.ApplyModifiedProperties())
                ((MapRoot)target).ApplyLighting();
        }

        private void DrawMapInfo()
        {
            string current = _mapNameProp.stringValue ?? "";
            if (current.Length > MaxLength)
                current = current.Substring(0, MaxLength);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Map Name", GUILayout.Width(EditorGUIUtility.labelWidth));
            string edited = EditorGUILayout.TextField(current);
            if (edited.Length > MaxLength)
                edited = edited.Substring(0, MaxLength);
            _mapNameProp.stringValue = edited;
            EditorGUILayout.EndHorizontal();

            int remaining = MaxLength - edited.Length;
            GUIStyle counter = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = remaining <= 3 ? new Color(0.9f, 0.3f, 0.3f)
                                        : remaining <= 6 ? new Color(0.95f, 0.7f, 0f)
                                        : new Color(0.55f, 0.55f, 0.55f) }
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("The name shown on the portal in-game.", _tooltipStyle);
            EditorGUILayout.LabelField($"{edited.Length} / {MaxLength}", counter, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(_portalColorProp, new GUIContent("Portal Color"));
            EditorGUILayout.LabelField("The colour of the portal and portal text players walk through to load your map.", _tooltipStyle);

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(_modsAllowedProp, new GUIContent("Mods Allowed"));
            EditorGUILayout.LabelField("Allow players to use mods while playing your map.", _tooltipStyle);

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(_maxPlayersProp, new GUIContent("Max Players"));
            EditorGUILayout.LabelField("Maximum number of players allowed in a public room running your map. Range 1-50.", _tooltipStyle);
        }

        private void DrawLighting()
        {
            EditorGUILayout.PropertyField(_skyboxModeProp, new GUIContent("Skybox"));
            EditorGUILayout.LabelField("Single Color fills the sky with a flat colour. Material uses a skybox material.", _tooltipStyle);

            EditorGUILayout.Space(6);

            bool isSingleColor = _skyboxModeProp.enumValueIndex == (int)MapRoot.SkyboxMode.SingleColor;

            if (isSingleColor)
            {
                EditorGUILayout.PropertyField(_skyboxColorProp, new GUIContent("Sky Color"));
                EditorGUILayout.LabelField("The flat colour used for the background sky. Please note that this is only visible in-game and not in the Unity editor!", _tooltipStyle);
            }
            else
            {
                EditorGUILayout.PropertyField(_skyboxMaterialProp, new GUIContent("Skybox Material"));
                EditorGUILayout.LabelField("A skybox material to use as the background.", _tooltipStyle);
            }

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(_ambientColorProp, new GUIContent("Ambient Color"));
            EditorGUILayout.LabelField("The colour of indirect light filling the scene. Keep dark for horror.", _tooltipStyle);

            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(_fogEnabledProp, new GUIContent("Fog"));
            EditorGUILayout.LabelField("Enable distance fog. Uses Linear mode.", _tooltipStyle);

            if (_fogEnabledProp.boolValue)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(_fogColorProp, new GUIContent("Fog Color"));
                EditorGUILayout.LabelField("The colour of the fog.", _tooltipStyle);

                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(_fogStartProp, new GUIContent("Start Distance"));
                EditorGUILayout.LabelField("Distance in metres where fog begins.", _tooltipStyle);

                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(_fogEndProp, new GUIContent("End Distance"));
                EditorGUILayout.LabelField("Distance in metres where fog is fully opaque.", _tooltipStyle);
            }
        }
    }
}
#endif