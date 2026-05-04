using System;
using UnityEngine;

[Serializable]
public class MapMeta
{
    public string mapName;
    public float  portalColorR;
    public float  portalColorG;
    public float  portalColorB;
    public float  portalColorA;
    public string androidBundle;
    public string win64Bundle;

    public Color PortalColor
    {
        get => new Color(portalColorR, portalColorG, portalColorB, portalColorA);
        set { portalColorR = value.r; portalColorG = value.g; portalColorB = value.b; portalColorA = value.a; }
    }

    public string BundleForCurrentPlatform()
    {
#if UNITY_EDITOR
        return win64Bundle;
#else
        return Application.platform == RuntimePlatform.Android ? androidBundle : win64Bundle;
#endif
    }
}
