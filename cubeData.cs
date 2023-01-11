using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class cubeData
{
    public readonly static int chunkX = 16;
    public readonly static int chunkY = 128;
    public readonly static int worldSize = 50;
    public readonly static int ViewDistance = 5;

    public readonly static int TextureSizeinBlocks = 4;
    public static float BlockSizeonTexture {
        get { return 1f / (float) TextureSizeinBlocks; }
    }
    public static readonly int AmountOfVoxelInWorld = worldSize * chunkX;

    public static readonly Vector3[] VoxelVerts = new Vector3[8] { 

        new Vector3( 0f, 0f, 0f ), 
        new Vector3( 1f, 0f, 0f ), 
        new Vector3( 1f, 1f, 0f ), 
        new Vector3( 0f, 1f, 0f ), 
        new Vector3( 0f, 0f, 1f ), 
        new Vector3( 1f, 0f, 1f ), 
        new Vector3( 1f, 1f, 1f ), 
        new Vector3( 0f, 1f, 1f ), 

    }; 

    public static readonly Vector3[] checkFaces = new Vector3[6] {
        new Vector3( 0.0f, 0.0f, -1.0f ), 
        new Vector3( 0.0f, 0.0f, 1.0f ), 
        new Vector3( 0.0f, 1.0f, 0.0f ), 
        new Vector3( 0.0f, -1.0f, 0.0f ), 
        new Vector3( -1.0f, 0.0f, 0.0f ), 
        new Vector3( 1.0f, 0.0f, 0.0f ), 
    };

    public static readonly int[,] VoxelTris = new int[6, 4] { 
        //0, 1, 2, 2, 1, 3
        {0, 3, 1, 2}, //back face         
        {5, 6, 4, 7}, //front face        
        {3, 7, 2, 6}, //top face         
        {1, 5, 0, 4}, //bottom face        
        {4, 7, 0, 3}, //left face        
        {1, 2, 5, 6}, //right face        
    };

    public static readonly int[,] VertsOrder = new int[1, 6] {
        { 0, 1, 2, 2, 1, 3 },
    };

    public static readonly Vector2[] CubeUvs = new Vector2[4] {
        new Vector2(0.0f, 0.0f), 
        new Vector2(0.0f, 1.0f), 
        new Vector2(1.0f, 0.0f), 
        new Vector2(1.0f, 1.0f), 
    };
}
