using System;
using System.Collections.Generic;

[Serializable]
public class MapScriptData
{
    public List<MapRootData> mapRoots = new();
    public List<MapAIData> mapAIs = new();
    public List<TeleporterData> teleporters = new();
    public List<JumpscareData> jumpscares = new();
    public List<ToggleData> togglers = new();
    public List<DoorData> doors = new();
    public List<LuaScriptData> luaScripts = new();
    public HitsoundData hitsounds = new();
}

[Serializable]
public class MapRootData
{
    public string objectPath;
    public string mapName;
    public float portalColorR;
    public float portalColorG;
    public float portalColorB;
    public float portalColorA;
    public bool modsAllowed;
    public int maxPlayers;
    public int skyboxMode;
    public string skyboxMaterialName;
    public float skyboxColorR;
    public float skyboxColorG;
    public float skyboxColorB;
    public float ambientColorR;
    public float ambientColorG;
    public float ambientColorB;
    public bool fogEnabled;
    public float fogColorR;
    public float fogColorG;
    public float fogColorB;
    public float fogStart;
    public float fogEnd;
}

[Serializable]
public class MapAIData
{
    public string objectPath;
    public int aiType;
    public float wanderSpeed;
    public float chaseSpeed;
    public float chaseDistance;
    public float fieldOfViewAngle;
    public string[] waypointPaths;
}

[Serializable]
public class TeleporterData
{
    public string objectPath;
    public string[] teleportPointPaths;
}

[Serializable]
public class JumpscareData
{
    public string objectPath;
    public string[] respawnPaths;
}

[Serializable]
public class ToggleData
{
    public string objectPath;
    public string[] targetPaths;
    public int mode;
    public bool oneShot;
}

[Serializable]
public class DoorData
{
    public string objectPath;
    public string doorObjectPath;
    public float openPosX, openPosY, openPosZ;
    public float closedPosX, closedPosY, closedPosZ;
    public float speed;
    public bool autoOpen;
    public float autoOpenTimer;
    public int networkMode;
}

[Serializable]
public class LuaScriptData
{
    public string objectPath;
    public string scriptName;
    public string source;
    public List<LuaFieldData> fields = new();
}

[Serializable]
public class LuaFieldData
{
    public string name;
    public string type;
    public string objectPath;
    public string assetName;
    public float number;
    public bool boolean;
    public string text;
}

[Serializable]
public class HitsoundData
{
    public List<HitsoundEntry> entries = new();
}

[Serializable]
public class HitsoundEntry
{
    public string[] materialNames;
    public string[] soundNames;
}
