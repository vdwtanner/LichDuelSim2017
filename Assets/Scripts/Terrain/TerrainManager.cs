using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// The terrain manager saves and loads heightmap, splatmap, and other data for terrain rendering.
[RequireComponent (typeof (Terrain))]
public class TerrainManager : MonoBehaviour {

    protected BoardManager mParentBoard;

    protected Terrain mTerrain;
    protected bool mTerrainLoaded;

    // Use this for initialization
    void Start () {
        mParentBoard = GetComponentInParent<BoardManager>();
        if (mParentBoard == null)
            Debug.LogError("TerrainManager has no BoardManager parent!");
        mTerrain = GetComponent<Terrain>();

    }

    // assumes the file ptr in the stream is currently at the beginnning of the terrain data
    public void Save(FileStream stream) {
        TerrainData data = mTerrain.terrainData;
        // format of terrain section:
        // Terrain Width, Terrain Height, Terrain Length
        stream.Write(BitConverter.GetBytes(data.size.x), 0, sizeof(float));
        stream.Write(BitConverter.GetBytes(data.size.y), 0, sizeof(float));
        stream.Write(BitConverter.GetBytes(data.size.z), 0, sizeof(float));

        // Heightmap Resolution
        stream.Write(BitConverter.GetBytes(data.heightmapResolution), 0, sizeof(int));

        // Splattmap/alphamap Resolution
        stream.Write(BitConverter.GetBytes(data.detailResolution), 0, sizeof(int));

        // Raw Data for heightmap
        for (int i = 0; i < data.heightmapWidth; i++) {
            for (int j = 0; j < data.heightmapHeight; j++) {
                float h = data.GetHeights(j, i, 1, 1)[0,0];
                byte[] bytes = BitConverter.GetBytes(h);
                stream.Write(bytes, 0, sizeof(float));
            }
        }

    }


    public void Load(FileStream stream) {
        TerrainData data = mTerrain.terrainData;
        byte[] temp = new byte[4];

        // Read Size
        stream.Read(temp, 0, sizeof(float));
        float x = BitConverter.ToSingle(temp, 0);
        stream.Read(temp, 0, sizeof(float));
        float y = BitConverter.ToSingle(temp, 0);
        stream.Read(temp, 0, sizeof(float));
        float z = BitConverter.ToSingle(temp, 0);
        data.size = new Vector3(x, y, z);

        // Read Heightmap resolution
        stream.Read(temp, 0, sizeof(int));
        data.heightmapResolution = BitConverter.ToInt32(temp, 0);

        // Read splattmap/alphamap Resolution
        stream.Read(temp, 0, sizeof(int));
        data.alphamapResolution = BitConverter.ToInt32(temp, 0);

        // Read raw data
        float[,] heights = new float[data.heightmapResolution, data.heightmapResolution];
        for (int i = 0; i < data.heightmapWidth; i++) {
            for (int j = 0; j < data.heightmapHeight; j++) {
                stream.Read(temp, 0, sizeof(float));
                heights[i, j] = BitConverter.ToSingle(temp, 0);
            }
        }

        data.SetHeights(0, 0, heights);
    }
}
