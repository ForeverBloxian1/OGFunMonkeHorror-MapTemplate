using UnityEngine;
using UnityEngine.AI;

namespace OGFunMonkeHorror.Assets.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MapAI : MonoBehaviour
    {
        public enum AIType { Neutral, Monster }

        [Header("AI Type")]
        public AIType aiType = AIType.Neutral;

        [Header("Movement")]
        public float wanderSpeed = 2f;
        public float chaseSpeed = 7f;

        [Header("Waypoints")]
        public GameObject[] waypoints;

        [Header("Detection")]
        public float chaseDistance = 30f;
        [Range(0f, 360f)]
        public float fieldOfViewAngle = 180f;

        [Header("Audio")]
        public AudioSource chaseAudio;

        private void OnDrawGizmosSelected()
        {
            if (aiType != AIType.Monster) return;

            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.12f);
            Gizmos.DrawSphere(transform.position, chaseDistance);
            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, chaseDistance);

            Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
            Vector3 leftDir = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftDir * chaseDistance);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * chaseDistance);

            int segments = 32;
            float angleStep = fieldOfViewAngle / segments;
            float start = -fieldOfViewAngle * 0.5f;
            for (int i = 0; i < segments; i++)
            {
                Vector3 p1 = transform.position + Quaternion.Euler(0, start + angleStep * i, 0) * transform.forward * chaseDistance;
                Vector3 p2 = transform.position + Quaternion.Euler(0, start + angleStep * (i + 1), 0) * transform.forward * chaseDistance;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}

#if UNITY_EDITOR
namespace OGFunMonkeHorror.Assets.AI.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(MapAI))]
    public class MapAIEditor : Editor
    {
        private SerializedProperty _aiType;
        private SerializedProperty _wanderSpeed;
        private SerializedProperty _chaseSpeed;
        private SerializedProperty _waypoints;
        private SerializedProperty _chaseDistance;
        private SerializedProperty _visibilityFactor;
        private SerializedProperty _fieldOfViewAngle;
        private SerializedProperty _chaseAudio;

        private void OnEnable()
        {
            _aiType = serializedObject.FindProperty("aiType");
            _wanderSpeed = serializedObject.FindProperty("wanderSpeed");
            _chaseSpeed = serializedObject.FindProperty("chaseSpeed");
            _waypoints = serializedObject.FindProperty("waypoints");
            _chaseDistance = serializedObject.FindProperty("chaseDistance");
            _visibilityFactor = serializedObject.FindProperty("visibilityFactor");
            _fieldOfViewAngle = serializedObject.FindProperty("fieldOfViewAngle");
            _chaseAudio = serializedObject.FindProperty("chaseAudio");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool isMonster = (MapAI.AIType)_aiType.enumValueIndex == MapAI.AIType.Monster;

            EditorGUILayout.LabelField("AI Type", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_aiType, new GUIContent("Type"));

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(_wanderSpeed, new GUIContent("Wander Speed"));
            if (isMonster)
                EditorGUILayout.PropertyField(_chaseSpeed, new GUIContent("Chase Speed"));

            EditorGUILayout.Space(10);

            if (isMonster)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Gizmos visible when this object is selected in the Scene view.", MessageType.None);
                EditorGUILayout.PropertyField(_chaseDistance, new GUIContent("Chase Distance"));
                EditorGUILayout.PropertyField(_fieldOfViewAngle, new GUIContent("Field of View"));

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(_chaseAudio, new GUIContent("Chase Audio"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif