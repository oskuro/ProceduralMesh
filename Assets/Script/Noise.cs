using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    
    public static float GetNoise(float seed, float x, float z)
    {
        float nx = (float)x + seed;
        float nz = (float)z + seed;

        float ny = Mathf.PerlinNoise(nx * 1.01f, nz * 1.01f);

        
        return ny;
    }
}
