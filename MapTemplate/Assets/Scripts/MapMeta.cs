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
    public string androidAssetsBundle;

    public Color PortalColor
    {
        get => new Color(portalColorR, portalColorG, portalColorB, portalColorA);
        set { portalColorR = value.r; portalColorG = value.g; portalColorB = value.b; portalColorA = value.a; }
    }
}
