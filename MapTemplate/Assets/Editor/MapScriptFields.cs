using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public static class MapScriptFields
{
    public static readonly Dictionary<string, Type> ObjectTypes = new()
    {
        { "GameObject", typeof(GameObject) },
        { "Transform", typeof(Transform) },
        { "TMP_Text", typeof(TMPro.TMP_Text) },
        { "AudioSource", typeof(AudioSource) },
        { "AudioClip", typeof(AudioClip) },
        { "Material", typeof(Material) },
        { "Light", typeof(Light) },
        { "Animator", typeof(Animator) },
        { "Rigidbody", typeof(Rigidbody) },
        { "Renderer", typeof(Renderer) },
        { "ParticleSystem", typeof(ParticleSystem) },
    };

    public struct Decl
    {
        public string name;
        public string type;
    }

    public static List<Decl> Parse(string src)
    {
        var list = new List<Decl>();
        if (string.IsNullOrEmpty(src)) return list;

        var block = Regex.Match(src, @"fields\s*=\s*\{([^}]*)\}", RegexOptions.Singleline);
        if (!block.Success) return list;

        foreach (var rawLine in block.Groups[1].Value.Split('\n'))
        {
            var line = rawLine.Trim().TrimEnd(',').Trim();
            if (line.Length == 0 || line.StartsWith("--")) continue;

            var entry = Regex.Match(line, @"^([A-Za-z_]\w*)\s*=\s*(.+)$");
            if (!entry.Success) continue;

            list.Add(new Decl { name = entry.Groups[1].Value, type = Classify(entry.Groups[2].Value.Trim()) });
        }
        return list;
    }

    static string Classify(string val)
    {
        if (val.Length == 0) return "string";
        if ((val[0] == '"' && val.EndsWith("\"")) || (val[0] == '\'' && val.EndsWith("'"))) return "string";
        if (val == "true" || val == "false") return "bool";
        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) return "number";

        var word = Regex.Match(val, @"^[A-Za-z_]\w*");
        return word.Success ? word.Value : "string";
    }

    public static bool IsObjectType(string type) => type != null && ObjectTypes.ContainsKey(type);
    public static bool AllowSceneObject(string type) => type != "AudioClip" && type != "Material";
    public static Type UnityType(string type) => (type != null && ObjectTypes.TryGetValue(type, out var t)) ? t : null;
}
