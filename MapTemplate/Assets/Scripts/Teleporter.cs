using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Teleporter : MonoBehaviour
{
    public Transform[] teleportPoints;

    private void OnValidate()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;

            if (col is MeshCollider mesh)
                mesh.convex = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (teleportPoints == null) return;

        Gizmos.color = Color.cyan;
        foreach (var point in teleportPoints)
        {
            if (point == null) continue;
            Gizmos.DrawWireSphere(point.position, 0.3f);
            Gizmos.DrawLine(transform.position, point.position);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Teleporter))]
public class TeleporterEditor : UnityEditor.Editor
{
    private SerializedProperty _teleportPoints;

    private void OnEnable()
    {
        _teleportPoints = serializedObject.FindProperty("teleportPoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var t = (Teleporter)target;
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

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_teleportPoints, new GUIContent("Teleport Points"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif