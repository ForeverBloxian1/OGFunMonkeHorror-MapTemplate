using System;
using System.Collections.Generic;

[Serializable]
public class MapScriptData
{
    public List<MapRootData> mapRoots = new();
    public List<MapAIData> mapAIs = new();
    public List<TeleporterData> teleporters = new();
    public List<JumpscareData> jumpscares = new();
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