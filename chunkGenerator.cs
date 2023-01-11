using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chunkGenerator
{
    public ChunkPos coords;
    GameObject chunk;
    MeshRenderer meshRender;
    MeshFilter meshFilter;

    int VI = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();  
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] chunkMap = new byte[cubeData.chunkX, cubeData.chunkY, cubeData.chunkX]; 

    worldScript world;

    bool _isChunkActive;

    public bool isVoxelMapPop = false;

    public Vector3 position { 
        get { return chunk.transform.position; }
    }

    public chunkGenerator (ChunkPos _coords, worldScript _world, bool RenderOnLoading) 
    {
        world = _world;
        coords = _coords;
        activeValue = true;


        if(RenderOnLoading) {
            RenderChunk();
        }

        /*chunk = new GameObject();
        meshFilter = chunk.AddComponent<MeshFilter>();
        meshRender = chunk.AddComponent<MeshRenderer>();
        meshRender.material = world.material;
        chunk.transform.SetParent(world.transform);
        chunk.transform.position = new Vector3(coords.x * cubeData.chunkX, 0f, coords.z * cubeData.chunkX);

        PopulateChunkMap();
        CreateChunk();
        ChunkRender();*/
    }

    public void RenderChunk() {
        chunk = new GameObject();
        meshFilter = chunk.AddComponent<MeshFilter>();
        meshRender = chunk.AddComponent<MeshRenderer>();
        meshRender.material = world.material;
        chunk.transform.SetParent(world.transform);
        chunk.transform.position = new Vector3(coords.x * cubeData.chunkX, 0f, coords.z * cubeData.chunkX);

        PopulateChunkMap();
        updateChunk();
    }


    void PopulateChunkMap()
    {
        for(int y = 0; y < cubeData.chunkY; y++) {
            for(int x = 0; x < cubeData.chunkX; x++) {
                for(int z = 0; z < cubeData.chunkX; z++) {
                    chunkMap[x, y, z] = world.Voxel(new Vector3(x, y, z) + position);
                }
            }
        }
        isVoxelMapPop = true;
    }

    void updateChunk()
    {
        ClearMesh();
        for (int y = 0; y < cubeData.chunkY; y++)
        {
            for (int x = 0; x < cubeData.chunkX; x++)
            {
                for (int z = 0; z < cubeData.chunkX; z++)
                {
                    if(world.blocks[chunkMap[x, y, z]].isSolid)
                    {
                        ChunkData(new Vector3(x, y, z));
                    }
                }
            }
        }
        ChunkRender();
    }


    void ClearMesh() {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        VI = 0;
    }

    bool VoxelInChunk(int x, int y, int z) {
        if(x < 0 || x > cubeData.chunkX - 1 || y < 0 || y > cubeData.chunkY - 1 || z < 0 || z > cubeData.chunkX - 1) {
            return false;
        }
        else { return true; }
    }


    public void ChangeChunkMap(Vector3 pos, byte blockID) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x = (int)(x - chunk.transform.position.x);
        z = (int)(z - chunk.transform.position.z);
        chunkMap[x, y, z] = blockID;
        if(chunkMap[x, y - 1, z] == 1) {
            chunkMap[x, y - 1, z] = 2;
        }

        updateOtherChunk(x, y, z);
        updateChunk();
    }

    void updateOtherChunk(int x, int y, int z) {
        Vector3 pos = new Vector3(x, y, z);
        for (int p = 0; p < 6; p++)
        {
            Vector3 current = pos + cubeData.checkFaces[p];
            if(!VoxelInChunk((int)current.x, (int)current.y, (int)current.z)) {
                world.GetChunk(current + position).updateChunk();
            }
        }
    }

    bool CheckChunkMap(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if(!VoxelInChunk(x, y, z)) {
            //return world.blocks[world.Voxel(pos + position)].isSolid;
            return world.CheckVoxel(pos + position);
        }

        return world.blocks[chunkMap[x, y, z]].isSolid;
    }

    public byte GetVoxelFromPos (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        x -= Mathf.FloorToInt(chunk.transform.position.x);
        z -= Mathf.FloorToInt(chunk.transform.position.z);

        return chunkMap[x, y, z];
    }

    void ChunkData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if(!CheckChunkMap(pos + cubeData.checkFaces[p])) {

                byte block = chunkMap[(int)pos.x, (int)pos.y, (int)pos.z];

                for(int i = 0; i < 4; i++) {
                    vertices.Add(pos + cubeData.VoxelVerts[cubeData.VoxelTris[p, i]]);
                }

                Texture(world.blocks[block].faceID[p]);

                for(int h = 0; h < 6; h++) {
                    triangles.Add(VI + cubeData.VertsOrder[0, h]);
                }
                VI += 4;
            }
        }
    }

    void Texture(int textureID) {
        float x = (float)textureID / cubeData.TextureSizeinBlocks - ((int) textureID / cubeData.TextureSizeinBlocks);
        float y = (float)((int) textureID / cubeData.TextureSizeinBlocks) / cubeData.TextureSizeinBlocks;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + cubeData.BlockSizeonTexture));
        uvs.Add(new Vector2(x + cubeData.BlockSizeonTexture, y));
        uvs.Add(new Vector2(x + cubeData.BlockSizeonTexture, y + cubeData.BlockSizeonTexture));
    }

    void ChunkRender()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    } 


    public bool activeValue {
        get { return _isChunkActive; }
        set { 
            _isChunkActive = value;
            if(chunk != null) { chunk.SetActive(value); }
        }
    }
}

public class ChunkPos {
    public int x;
    public int z;
    public ChunkPos(int _x, int _z) {
        x = _x;
        z = _z;
    }
}

public class activeData {
}
