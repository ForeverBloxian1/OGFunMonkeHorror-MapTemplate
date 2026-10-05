using System;
using System.Collections.Generic;
using Lua.Unity;
using UnityEngine;

[AddComponentMenu("OG Fun Monke Horror/Map Script")]
public class MapScript : MonoBehaviour
{
    [Serializable]
    public class ScriptField
    {
        public string name;
        public string type;
        public UnityEngine.Object objectValue;
        public float numberValue;
        public bool boolValue;
        public string stringValue = "";
    }

    public LuaAsset script;
    public List<ScriptField> fields = new();
}
