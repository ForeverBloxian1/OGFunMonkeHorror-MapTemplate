using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class DoorTrigger : MonoBehaviour
{
    public enum NetworkMode
    {
        ClientSide,
        ServerSide,
    }

    public Transform doorObject;
    public Vector3 openPosition;
    public Vector3 closedPosition;
    public float speed = 1f;

    public AudioClip buttonAudio;
    public AudioClip doorAudio;

    public bool autoOpen = false;
    public float autoOpenTimer = 5f;

    public NetworkMode networkMode = NetworkMode.ServerSide;

    private void OnValidate()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        var col = GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        var src = GetComponent<AudioSource>();
        if (src != null)
        {
            src.playOnAwake = false;
            src.spatialBlend = 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (doorObject == null) return;

        Vector3 worldClosed = doorObject.parent != null
            ? doorObject.parent.TransformPoint(closedPosition)
            : closedPosition;
        Vector3 worldOpen = doorObject.parent != null
            ? doorObject.parent.TransformPoint(openPosition)
            : openPosition;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldClosed, Vector3.one * 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(worldOpen, Vector3.one * 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldClosed, worldOpen);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DoorTrigger))]
public class DoorTriggerEditor : UnityEditor.Editor
{
    private UnityEditor.SerializedProperty _doorObject;
    private UnityEditor.SerializedProperty _openPosition;
    private UnityEditor.SerializedProperty _closedPosition;
    private UnityEditor.SerializedProperty _speed;
    private UnityEditor.SerializedProperty _buttonAudio;
    private UnityEditor.SerializedProperty _doorAudio;
    private UnityEditor.SerializedProperty _autoOpen;
    private UnityEditor.SerializedProperty _autoOpenTimer;
    private UnityEditor.SerializedProperty _networkMode;

    private void OnEnable()
    {
        _doorObject = serializedObject.FindProperty("doorObject");
        _openPosition = serializedObject.FindProperty("openPosition");
        _closedPosition = serializedObject.FindProperty("closedPosition");
        _speed = serializedObject.FindProperty("speed");
        _buttonAudio = serializedObject.FindProperty("buttonAudio");
        _doorAudio = serializedObject.FindProperty("doorAudio");
        _autoOpen = serializedObject.FindProperty("autoOpen");
        _autoOpenTimer = serializedObject.FindProperty("autoOpenTimer");
        _networkMode = serializedObject.FindProperty("networkMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var t = (DoorTrigger)target;
        var col = t.GetComponent<BoxCollider>();
        bool layerOk = t.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast");

        if (!layerOk)
            UnityEditor.EditorGUILayout.HelpBox("Layer must be set to Ignore Raycast.", UnityEditor.MessageType.Error);

        if (col == null)
            UnityEditor.EditorGUILayout.HelpBox("A BoxCollider component is required.", UnityEditor.MessageType.Error);
        else if (!col.isTrigger)
            UnityEditor.EditorGUILayout.HelpBox("BoxCollider must have Is Trigger enabled.", UnityEditor.MessageType.Error);

        if (t.doorObject == null)
            UnityEditor.EditorGUILayout.HelpBox("Assign the door GameObject in the Door Object field.", UnityEditor.MessageType.Error);

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_doorObject);
        UnityEditor.EditorGUILayout.PropertyField(_openPosition);
        UnityEditor.EditorGUILayout.PropertyField(_closedPosition);
        UnityEditor.EditorGUILayout.PropertyField(_speed);

        UnityEditor.EditorGUILayout.Space(6);
        UnityEditor.EditorGUILayout.LabelField("Audio", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(_buttonAudio, new GUIContent("Button Audio Clip"));
        UnityEditor.EditorGUILayout.PropertyField(_doorAudio, new GUIContent("Door Audio Clip"));

        UnityEditor.EditorGUILayout.Space(6);
        UnityEditor.EditorGUILayout.LabelField("Auto Open", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(_autoOpen);
        if (_autoOpen.boolValue)
            UnityEditor.EditorGUILayout.PropertyField(_autoOpenTimer, new GUIContent("Auto Open Timer (s)"));

        UnityEditor.EditorGUILayout.Space(6);
        UnityEditor.EditorGUILayout.LabelField("Network", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.PropertyField(_networkMode);
        UnityEditor.EditorGUILayout.LabelField(
            t.networkMode == DoorTrigger.NetworkMode.ServerSide
                ? "Server-side doors stay in sync for everyone in the room."
                : "Client-side doors are not networked. Each player sees their own door state.",
            UnityEditor.EditorStyles.miniLabel);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
