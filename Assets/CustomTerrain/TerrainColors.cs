using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TerrainColors
{
    public ColorNames name;
    public Color color;
}

public enum ColorNames
{
    Water,
    Sand,
    Grass,
    Forest,
    Mountain,
    Snow
}
