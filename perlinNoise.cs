using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perlinNoise
{
    public static float Noise2D (Vector2 pos, float offset, float scale) {
        return Mathf.PerlinNoise((pos.x + 0.1f) / cubeData.chunkX * scale + offset, (pos.y + 0.1f) / cubeData.chunkX * scale + offset);
    }
}
