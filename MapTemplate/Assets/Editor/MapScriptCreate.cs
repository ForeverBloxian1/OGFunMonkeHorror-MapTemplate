using UnityEditor;

public static class MapScriptCreate
{
    public const string Scaffold =
@"
";

    [MenuItem("Assets/Create/OG Fun Monke Horror/Map Script (Lua)", false, 80)]
    public static void CreateLuaScript()
    {
        ProjectWindowUtil.CreateAssetWithContent("NewMapScript.lua", Scaffold);
    }
}
