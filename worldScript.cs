using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class worldScript : MonoBehaviour
{
    public int seed;
    public Material material;
    public Blocks[] blocks;
    public Transform playerTransform;
    public Vector3 spawn { 
        get {
            return new Vector3(cubeData.worldSize * cubeData.chunkX / 2f, Mathf.FloorToInt(42 * perlinNoise.Noise2D(new Vector2(cubeData.worldSize * cubeData.chunkX / 2f, cubeData.worldSize * cubeData.chunkX / 2f), 500, 0.25f) + 42) + 3f, cubeData.worldSize * cubeData.chunkX / 2f);
        }
    }

    chunkGenerator[,] worldChunk = new chunkGenerator[cubeData.worldSize, cubeData.worldSize];
    Vector3 PreviousPlayerChunk;

    List<Vector3> chunksOn = new List<Vector3>();
    List<Vector3> chunksToLoad = new List<Vector3>();
    bool loadingChunks = false;
 
    public TextMeshProUGUI FpsText;
    private float fpsTime = 1f;
    private float time;
    private int frames;

    private void Start() {
        Random.InitState(seed);
        playerTransform.position = spawn;
        WorldGenerator();
        PreviousPlayerChunk = new Vector3((int)playerTransform.position.x, playerTransform.position.y, (int)playerTransform.position.z);
    }

    private void Update() {
        NewChunk();

        if(chunksToLoad.Count > 0 && !loadingChunks) {
            StartCoroutine("LoadChunks");
        }

        time += Time.deltaTime;
        frames++;
        if(time >= fpsTime) {
            int framesPerSec = Mathf.RoundToInt(frames / time);
            FpsText.text = "FPS: " + framesPerSec;
            frames = 0;
            time -= fpsTime;
        }
    }

    //Create chunks when the player spawn
    void WorldGenerator () {
        for (int x = (int)(cubeData.worldSize / 2f) - cubeData.ViewDistance; x < (int)(cubeData.worldSize / 2f) + cubeData.ViewDistance; x++) {
            for (int z = (int)(cubeData.worldSize / 2f) - cubeData.ViewDistance; z < (int)(cubeData.worldSize / 2f) + cubeData.ViewDistance; z++) {
                worldChunk[x, z] = new chunkGenerator(new ChunkPos(x, z), this, true);  
                chunksOn.Add(new Vector3(x, 0f, z));
            }           
        }    
    }

    IEnumerator LoadChunks() {
        loadingChunks = true;
        while(chunksToLoad.Count > 0) {
            worldChunk[(int)chunksToLoad[0].x, (int)chunksToLoad[0].z].RenderChunk();

            chunksToLoad.RemoveAt(0);

            yield return null;
        }
        loadingChunks = false;
    }

    public ChunkPos GetPosition (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / cubeData.chunkX); int z = Mathf.FloorToInt(pos.z / cubeData.chunkX);
        return new ChunkPos(x, z);
    }


    //Render and Unrender chunks next to player
    void NewChunk() {
        int xPos = Mathf.FloorToInt(playerTransform.position.x / cubeData.chunkX); 
        int zPos = Mathf.FloorToInt(playerTransform.position.z / cubeData.chunkX); 

        if((int) PreviousPlayerChunk.x / cubeData.chunkX != xPos || (int) PreviousPlayerChunk.z / cubeData.chunkX != zPos) 
        {
            List<Vector3> previousChunksOn = new List<Vector3>(chunksOn);

            for (int x = xPos - cubeData.ViewDistance; x < xPos + cubeData.ViewDistance; x++) {
                for (int z = zPos - cubeData.ViewDistance; z < zPos + cubeData.ViewDistance; z++) {
                    if(ChunkIsIntheWorld(x, z)) 
                    {
                        if(worldChunk[x, z] == null)
                        {
                            worldChunk[x, z] = new chunkGenerator(new ChunkPos(x, z), this, false); 
                            chunksToLoad.Add(new Vector3(x, 0f, z));
                        }
                        else if(!worldChunk[x, z].activeValue) 
                        {
                            worldChunk[x, z].activeValue = true;
                        }
                        chunksOn.Add(new Vector3(x, 0f, z));
                    }

                    for (int i = 0; i < previousChunksOn.Count; i++)
                    {
                        if(previousChunksOn[i].x == x && previousChunksOn[i].z == z) {
                            previousChunksOn.RemoveAt(i);
                        }
                    }
                }
            }
            foreach (Vector3 chunk in previousChunksOn)
            {
                worldChunk[(int)chunk.x, (int)chunk.z].activeValue = false;
            }

            PreviousPlayerChunk = playerTransform.position;
        }
    }

    //Chunk Preset (add perlin noise to make terrain)
    public byte Voxel(Vector3 pos)
    {
        int y = Mathf.FloorToInt(pos.y);
        if(!VoxelIsInTheWorld(pos)) { return 0;}

        if(y == 0) {
            return 4;
        }

        int terrain = Mathf.FloorToInt(42 * perlinNoise.Noise2D(new Vector2(pos.x, pos.z), 500, 0.25f) + 42);
        if(y == terrain) {
            return 1;
        }
        if(y < terrain && y > terrain - 5) {
            return 2;
        }
        else if (y > terrain) {
            return 0;
        }
        else { return 3; }
        
    }

    public bool CheckVoxel (Vector3 pos) {
        int xC = Mathf.FloorToInt(pos.x);
        int zC = Mathf.FloorToInt(pos.z);
         int x = xC / cubeData.chunkX;
         int z = zC / cubeData.chunkX;

        if(!ChunkIsIntheWorld(x, z) || pos.y < 0f || pos.y > cubeData.chunkY) {
            return false;
        }
        if(worldChunk[x, z] != null && worldChunk[x, z].isVoxelMapPop) {
            return blocks[worldChunk[x, z].GetVoxelFromPos(pos)].isSolid;
        }
        return blocks[Voxel(pos)].isSolid;
    }

    public chunkGenerator GetChunk(Vector3 pos) {
        return worldChunk[Mathf.FloorToInt(pos.x / cubeData.chunkX), Mathf.FloorToInt(pos.z / cubeData.chunkX)];
    }

    //Check if the chunk is in the world
    public bool ChunkIsIntheWorld(int x, int z) {
        if(x >= 0 && x < cubeData.worldSize && z >= 0 && z < cubeData.worldSize) { return true; }
        else { return false; }
    }

    //Check if the voxel is in the world
    public bool VoxelIsInTheWorld (Vector3 pos) {
        if(pos.x >= 0 && pos.x < cubeData.AmountOfVoxelInWorld && pos.y >= 0 && pos.y < cubeData.chunkY && pos.z >= 0 && pos.z < cubeData.AmountOfVoxelInWorld) {
            return true;
        }
        else {
            return false;
        }
    }
}

//Block Data (create the same system with scriptable objects later)
[System.Serializable]
public class Blocks
{
    public string Name;
    public bool isSolid;
    [Header("back, front, top, bottom, left, right")]
    public int[] faceID = new int[6];
}
