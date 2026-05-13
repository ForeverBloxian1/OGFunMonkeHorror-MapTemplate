using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnOnTriggered : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;
    public bool useTriggerTransformAsSpawn = false;
    public bool oneShot = true;

    private void OnValidate()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            if (col is MeshCollider mesh) mesh.convex = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 pos = useTriggerTransformAsSpawn || spawnPoint == null
            ? transform.position
            : spawnPoint.position;
        Gizmos.DrawWireSphere(pos, 0.3f);
        if (!useTriggerTransformAsSpawn && spawnPoint != null)
            Gizmos.DrawLine(transform.position, spawnPoint.position);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SpawnOnTriggered))]
public class SpawnOnTriggeredEditor : UnityEditor.Editor
{
    private UnityEditor.SerializedProperty _prefab;
    private UnityEditor.SerializedProperty _spawnPoint;
    private UnityEditor.SerializedProperty _useTriggerTransform;
    private UnityEditor.SerializedProperty _oneShot;

    private void OnEnable()
    {
        _prefab = serializedObject.FindProperty("prefab");
        _spawnPoint = serializedObject.FindProperty("spawnPoint");
        _useTriggerTransform = serializedObject.FindProperty("useTriggerTransformAsSpawn");
        _oneShot = serializedObject.FindProperty("oneShot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var t = (SpawnOnTriggered)target;
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

        if (t.prefab == null)
            UnityEditor.EditorGUILayout.HelpBox("Assign a Prefab or this trigger will do nothing.", UnityEditor.MessageType.Warning);

        if (!t.useTriggerTransformAsSpawn && t.spawnPoint == null)
            UnityEditor.EditorGUILayout.HelpBox("Assign a Spawn Point or enable Use Trigger Transform.", UnityEditor.MessageType.Warning);

        UnityEditor.EditorGUILayout.Space(4);
        UnityEditor.EditorGUILayout.PropertyField(_prefab);
        UnityEditor.EditorGUILayout.PropertyField(_useTriggerTransform, new GUIContent("Use Trigger Transform", "Spawn at this trigger's own position instead of a separate Spawn Point."));
        if (!t.useTriggerTransformAsSpawn)
            UnityEditor.EditorGUILayout.PropertyField(_spawnPoint);
        UnityEditor.EditorGUILayout.PropertyField(_oneShot);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
