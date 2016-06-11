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

    public enum HexTextureType {
        Default = 0,
        Valid = 1,
        Attack = 2
    }

    // distance from one vertex to its opposite vertex
    [Header("Hex Settings")]
    public Material hexMaterial;
    public float hexSize = 1.0f;
    public int hexChunkSize = 16;
    public float offsetFromTerrain = 0.1f;
    [Header("Hex Validity")]
    public float maxVariance = 0.3f;
    [Range(0.0f, 1.0f)] public float coplanarTolerance = 0.2f;
    [Header("Textures")]
    public Texture defaultTex;
    public Texture validMoveTex;
    public Texture attackRangeTex;


    private Texture2D mAtlasTexture;
    private Rect[] mAtlas;
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
        Texture2D[] texArray = new Texture2D[2];
        texArray[(int)HexTextureType.Default] = defaultTex as Texture2D;
        texArray[(int)HexTextureType.Valid] = validMoveTex as Texture2D;
        //texArray[(int)HexTextureType.Attack] = attackRangeTex as Texture2D;

        mAtlasTexture = new Texture2D(1048, 1048);
        mAtlas = mAtlasTexture.PackTextures(texArray, 2, 1048);
        Debug.Log(mAtlas);

        for (int i = 0; i < mAtlas.GetLength(0); i++) {
            Debug.Log(mAtlas[i]);
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


    Vector2 GetHexIndexFromWorldPos(Vector3 worldPos) {
        Vector3 chunklocal = worldPos - transform.position - transform.parent.position;

        // x position is difficult
        // start by finding its position with a uniform x axis, where the two non-uniform lengths are combined
        float uniform2X = chunklocal.x / (hexSize + mHexSideLength);
        float extra = uniform2X - Mathf.Floor(uniform2X);
        // figure out at what value of extra we're in the smaller length hex
        float smallerHexThresh = hexSize / (hexSize + mHexSideLength);
        int finalX = (int)(Mathf.Floor(uniform2X) * 2);
        if (extra >= smallerHexThresh)
            finalX += 1;

        // y position is easy, but we gotta remember that offset every other column yo
        int finalY = 0;
        if (finalX % 2 == 0) {
            finalY = (int)(chunklocal.z / mHexEdgeToEdgeLength);
        } else {
            finalY = (int)((chunklocal.z / mHexEdgeToEdgeLength) + (mHexEdgeToEdgeLength / 2));
        }

        return new Vector2(finalX, finalY);
    }

    public void SetHexTexture(Vector3 worldPos, HexTextureType texture) {
        Vector2 idx = GetHexIndexFromWorldPos(worldPos);
        int chunkIdxX = (int)idx.x / hexChunkSize;
        int chunkIdxY = (int)idx.y / hexChunkSize;
        mChunkList[chunkIdxX, chunkIdxY].SetHexUV((int)idx.x % hexChunkSize, (int)idx.y % hexChunkSize, mAtlas[(int)texture]);
    }

    public Terrain getTerrain() {
        return hTerrain;
    }
}
