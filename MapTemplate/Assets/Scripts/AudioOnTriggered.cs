using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class AudioOnTriggered : MonoBehaviour
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public bool loop = false;
    public bool oneShot = false;

    private void OnValidate()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            if (col is MeshCollider mesh) mesh.convex = true;
        }

        var src = GetComponent<AudioSource>();
        if (src != null)
        {
            src.playOnAwake = false;
            src.spatialBlend = spatialBlend;
            src.volume = volume;
            src.loop = loop;
            src.clip = clip;
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AudioOnTriggered))]
public class AudioOnTriggeredEditor : UnityEditor.Editor
{
    private UnityEditor.SerializedProperty _clip;
    private UnityEditor.SerializedProperty _volume;
    private UnityEditor.SerializedProperty _spatialBlend;
    private UnityEditor.SerializedProperty _loop;
    private UnityEditor.SerializedProperty _oneShot;

    private void OnEnable()
    {
        _clip = serializedObject.FindProperty("clip");
        _volume = serializedObject.FindProperty("volume");
        _spatialBlend = serializedObject.FindProperty("spatialBlend");
        _loop = serializedObject.FindProperty("loop");
        _oneShot = serializedObject.FindProperty("oneShot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var t = (AudioOnTriggered)target;
        var col = t.GetComponent<Collider>();
        var mesh = col as MeshCollider;
        bool layerOk = t.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast");

        if (!layerOk)
            UnityEditor.EditorGUILayout.HelpBox("Layer must be set to Ignore Raycast.", UnityEditor.MessageType.Error);

        if (col == null)
            UnityEditor.EditorGUILayout.HelpBox("A Collider component is required.", UnityEditor.MessageType.Error);
        else
        {
            if (!col.isTrigger)
                UnityEditor.EditorGUILayout.HelpBox("Collider must have Is Trigger enabled.", UnityEditor.MessageType.Error);

            if (mesh != null && !mesh.convex)
                UnityEditor.EditorGUILayout.HelpBox("Mesh Collider must have Convex enabled.", UnityEditor.MessageType.Error);
        }

        if (t.clip == null)
            UnityEditor.EditorGUILayout.HelpBox("Assign an Audio Clip or this trigger will do nothing.", UnityEditor.MessageType.Warning);

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_clip);
        UnityEditor.EditorGUILayout.PropertyField(_volume);
        UnityEditor.EditorGUILayout.PropertyField(_spatialBlend, new GUIContent("Spatial Blend", "0 = 2D, 1 = full 3D positional audio"));
        UnityEditor.EditorGUILayout.PropertyField(_loop);
        UnityEditor.EditorGUILayout.PropertyField(_oneShot);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
