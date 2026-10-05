using System.Collections.Generic;
using Lua.Unity;
using UnityEditor;
using UnityEngine;

namespace OGFunMonkeHorror.Editor
{
    [CustomEditor(typeof(MapScript))]
    public class MapScriptEditor : UnityEditor.Editor
    {
        private string _lastValidated;
        private bool _hasValidated;
        private bool _isValid;
        private string _error;
        private string _lastFieldsText;

        public override void OnInspectorGUI()
        {
            var ms = (MapScript)target;

            EditorGUI.BeginChangeCheck();
            var newScript = (LuaAsset)EditorGUILayout.ObjectField("Lua Script", ms.script, typeof(LuaAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ms, "Assign Lua Script");
                ms.script = newScript;
                EditorUtility.SetDirty(ms);
            }

            if (ms.script == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a .lua script, or make one: right-click in the Project window → Create → OG Fun Monke Horror → Map Script (Lua).",
                    MessageType.Info);
                if (GUILayout.Button("Create New Script")) MapScriptCreate.CreateLuaScript();
                return;
            }

            string text = ms.script.Text;

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Edit Script")) AssetDatabase.OpenAsset(ms.script);

            if (!_hasValidated || text != _lastValidated)
            {
                _isValid = MapScriptValidator.Validate(text, out _error);
                _lastValidated = text;
                _hasValidated = true;
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(_isValid ? "Script compiles." : "Lua error:\n" + _error,
                _isValid ? MessageType.Info : MessageType.Error);

            if (text != _lastFieldsText)
            {
                ReconcileFields(ms, text);
                _lastFieldsText = text;
            }

            if (ms.fields != null && ms.fields.Count > 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
                DrawFields(ms);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextArea(text, GUILayout.MaxHeight(140));
        }

        private void ReconcileFields(MapScript ms, string text)
        {
            var decls = MapScriptFields.Parse(text);
            var newList = new List<MapScript.ScriptField>();
            foreach (var d in decls)
            {
                var existing = ms.fields?.Find(f => f != null && f.name == d.name);
                if (existing != null && existing.type == d.type)
                    newList.Add(existing);
                else
                    newList.Add(new MapScript.ScriptField { name = d.name, type = d.type });
            }

            bool changed = ms.fields == null || ms.fields.Count != newList.Count;
            if (!changed)
                for (int i = 0; i < newList.Count; i++)
                    if (!ReferenceEquals(ms.fields[i], newList[i])) { changed = true; break; }

            if (changed)
            {
                Undo.RecordObject(ms, "Update Script Fields");
                ms.fields = newList;
                EditorUtility.SetDirty(ms);
            }
        }

        private void DrawFields(MapScript ms)
        {
            foreach (var f in ms.fields)
            {
                if (f == null) continue;
                var label = new GUIContent(ObjectNames.NicifyVariableName(f.name), f.type);

                EditorGUI.BeginChangeCheck();
                if (MapScriptFields.IsObjectType(f.type))
                {
                    var value = EditorGUILayout.ObjectField(label, f.objectValue,
                        MapScriptFields.UnityType(f.type), MapScriptFields.AllowSceneObject(f.type));
                    if (EditorGUI.EndChangeCheck()) Apply(ms, () => f.objectValue = value);
                }
                else if (f.type == "number")
                {
                    var value = EditorGUILayout.FloatField(label, f.numberValue);
                    if (EditorGUI.EndChangeCheck()) Apply(ms, () => f.numberValue = value);
                }
                else if (f.type == "bool")
                {
                    var value = EditorGUILayout.Toggle(label, f.boolValue);
                    if (EditorGUI.EndChangeCheck()) Apply(ms, () => f.boolValue = value);
                }
                else
                {
                    var value = EditorGUILayout.TextField(label, f.stringValue);
                    if (EditorGUI.EndChangeCheck()) Apply(ms, () => f.stringValue = value);
                }
            }
        }

        private static void Apply(MapScript ms, System.Action set)
        {
            Undo.RecordObject(ms, "Edit Field");
            set();
            EditorUtility.SetDirty(ms);
        }
    }
}
