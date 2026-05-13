using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToggleOnTriggered : MonoBehaviour
{
    public enum Mode
    {
        Toggle,
        Enable,
        Disable,
    }

    public GameObject[] targets;
    public Mode mode = Mode.Toggle;
    public bool oneShot = false;

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
        if (targets == null) return;

        Gizmos.color = mode == Mode.Disable ? Color.red
                     : mode == Mode.Enable  ? Color.green
                     : Color.yellow;

        foreach (var go in targets)
        {
            if (go == null) continue;
            Gizmos.DrawLine(transform.position, go.transform.position);
            Gizmos.DrawWireSphere(go.transform.position, 0.2f);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ToggleOnTriggered))]
public class ToggleOnTriggeredEditor : UnityEditor.Editor
{
    private UnityEditor.SerializedProperty _targets;
    private UnityEditor.SerializedProperty _mode;
    private UnityEditor.SerializedProperty _oneShot;

    private void OnEnable()
    {
        _targets = serializedObject.FindProperty("targets");
        _mode = serializedObject.FindProperty("mode");
        _oneShot = serializedObject.FindProperty("oneShot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var t = (ToggleOnTriggered)target;
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

        if (t.targets == null || t.targets.Length == 0)
            UnityEditor.EditorGUILayout.HelpBox("Add at least one GameObject to the Targets list.", UnityEditor.MessageType.Warning);

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_mode);
        UnityEditor.EditorGUILayout.PropertyField(_oneShot);
        UnityEditor.EditorGUILayout.PropertyField(_targets, new GUIContent("Targets"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
