using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainHexGrid : MonoBehaviour {

    /*  This class overlays a hexgrid on top of the terrain, figures out which hexs are valid to walk on,
        and only draws them

    Pretty difficult problem.
    Main issues:
        the distance between the center of one hex and the center of the one to its left
        is different than the distance between the center of the that hex to the center of the one above.
        They don't lay out in a grid well.
        Therefore, it wont sync up with the heightmap grid well
    
    My algorithm:
        1. Generate a hex grid that best fits within the total area of the terrain
        2. Loop through each vertex of the grid and set its height equal to the value returned by the SampleHeight function
        3. Then loop through each hex of the grid and figure out if it is valid to walk in.

    */

    // distance from one vertex to its opposite vertex
    [Header("Hex Settings")]
    public Material hexMaterial;
    public float hexSize = 1.0f;
    public int hexChunkSize = 16;
    public float offsetFromTerrain = 0.1f;
    [Header("Hex Validity")]
    public float maxVariance = 0.3f;


    private float mHexSideLength;
    private float mHexEdgeToEdgeLength;

    // non-uniform size axis
    private int mGridWidth;
    // uniform size axis
    private int mGridDepth;

    private int mChunksWidth;
    private int mChunksDepth;

    private HexChunk[,] mChunkList;
    
    private Terrain hTerrain;

	// Use this for initialization
	void Start () {
        hTerrain = GetComponent<Terrain>();
        if (hTerrain == null)
            Debug.LogError("TerrainHexGrid has no terrain component on its gameobject!");
        if (hexChunkSize % 2 != 0)
            Debug.LogError("TerrainHexGrid hexChunkSize MUST be a multiple of 2.");

        // figure out our other lengths from the hexSize
        mHexSideLength = hexSize * Mathf.Cos((1.0f / 3.0f) * Mathf.PI);
        mHexEdgeToEdgeLength = hexSize * Mathf.Sin((1.0f / 3.0f) * Mathf.PI);

        float terrainWidth = hTerrain.terrainData.size.x;
        float terrainDepth = hTerrain.terrainData.size.z;
        mGridWidth = (int)((terrainWidth / (hexSize + mHexSideLength)) * 2);
        mGridDepth = (int)(terrainDepth / mHexEdgeToEdgeLength);

        mChunksWidth = (int)Mathf.Ceil(mGridWidth / (float)hexChunkSize);
        mChunksDepth = (int)Mathf.Ceil(mGridDepth / (float)hexChunkSize);

        float chunkWidth = (hexSize + mHexSideLength) * (hexChunkSize / 2);
        float chunkDepth = (mHexEdgeToEdgeLength) * hexChunkSize;
        mChunkList = new HexChunk[mChunksWidth, mChunksDepth];
        for (int i = 0; i < mChunksWidth; i++) {
            for (int j = 0; j < mChunksDepth; j++) {
                Vector3 pos = new Vector3();
                pos.x = i * chunkWidth;
                pos.y = hTerrain.transform.position.y;
                pos.z = j * chunkDepth;
                GameObject obj = new GameObject("chunkX" + i + "Z" + j);
                HexChunk chunk = obj.AddComponent<HexChunk>();
                chunk.Initialize(this, pos, hexChunkSize, hexSize, hexMaterial);
                chunk.transform.parent = this.transform;
                mChunkList[i, j] = chunk;
            }
        }

	}

    public void TerrainModified() {
        StartCoroutine(UpdateValidity());
    }

    private IEnumerator UpdateValidity() {
        for (int i = 0; i < mChunksWidth; i++) {
            for (int j = 0; j < mChunksDepth; j++) {
                mChunkList[i, j].UpdateValidity();
                yield return null;
            }
        }

        yield return null;

        StartCoroutine(UpdateMeshes());
    }

    private IEnumerator UpdateMeshes() {
        for (int i = 0; i < mChunksWidth; i++) {
            for (int j = 0; j < mChunksDepth; j++) {
                mChunkList[i, j].RebuildMesh();
                yield return null;
            }
        }

        yield return null;
    }

    public Terrain getTerrain() {
        return hTerrain;
    }
}
