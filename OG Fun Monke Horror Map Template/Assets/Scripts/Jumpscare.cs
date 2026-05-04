using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Jumpscare : MonoBehaviour
{
    public GameObject  jumpscarePrefab;
    public Transform[] respawnPoints;

    private void OnValidate()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (respawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (var point in respawnPoints)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.3f);
            Gizmos.DrawLine(transform.position, point.position);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Jumpscare))]
public class JumpscareEditor : UnityEditor.Editor
{
    private SerializedProperty _prefab;
    private SerializedProperty _respawnPoints;

    private void OnEnable()
    {
        _prefab        = serializedObject.FindProperty("jumpscarePrefab");
        _respawnPoints = serializedObject.FindProperty("respawnPoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var j = (Jumpscare)target;

        if (j.jumpscarePrefab == null)
            UnityEditor.EditorGUILayout.HelpBox("A jumpscare prefab must be assigned.", UnityEditor.MessageType.Error);

        if (j.respawnPoints == null || j.respawnPoints.Length == 0)
            UnityEditor.EditorGUILayout.HelpBox("At least 1 respawn point must be assigned.", UnityEditor.MessageType.Error);

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_prefab,        new GUIContent("Jumpscare Prefab"));
        UnityEditor.EditorGUILayout.PropertyField(_respawnPoints, new GUIContent("Respawn Points"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
