﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// MEMORY TO BE TRIMMED HERE. CURRENTLY CHUNKS THAT HANG OFF EDGES STILL CONTAIN EXTRA DISABLED HEXES INSTEAD OF
// MAKING THEIR HEX ARRAY SMALLER
public class HexChunk : MonoBehaviour {

    float mHexSize;
    float mHexSideLength;
    float mHexEdgeToEdgeLength;
    int mChunkSize;

    Hex[,] mHexArr;
	private Vector2 mIndexIntoGrid;

    private TerrainHexGrid hGrid;

    private Material mMaterial;
    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;
    private Mesh mMesh;

    private Vector3[] mHexVerts;
    private int[] mTriangles;
    private Vector2[] mHexUVs;

    private bool mIsLevelEditor;

    public void Initialize(TerrainHexGrid grid, Vector3 chunkPos, int chunkSize, float hexSize, Material mat, Vector2 chunkIndex) {
        transform.position = chunkPos;
        mChunkSize = chunkSize;
        mHexSize = hexSize;
		mIndexIntoGrid = chunkIndex;

        hGrid = grid;
        mMaterial = mat;

        // figure out our other lengths from the hexSize
        mHexSideLength = hexSize * Mathf.Cos((1.0f / 3.0f) * Mathf.PI);
        mHexEdgeToEdgeLength = hexSize * Mathf.Sin((1.0f / 3.0f) * Mathf.PI);

        float cornerToCornerX = mHexEdgeToEdgeLength * Mathf.Cos(Mathf.PI / 6.0f);

        mHexArr = new Hex[chunkSize, chunkSize];

        mMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        mMeshFilter = gameObject.AddComponent<MeshFilter>();
        mMesh = new Mesh();

        mMeshRenderer.sharedMaterial = mMaterial;

        mIsLevelEditor = true;

        for (int i = 0; i < chunkSize; i++) {
            for (int j = 0; j < chunkSize; j++) {
                float x, y, z;
                x = i * cornerToCornerX;
                y = chunkPos.y;
                z = j * mHexEdgeToEdgeLength;
                if (i % 2 != 0) {
                    z += (mHexEdgeToEdgeLength / 2);
                }
                Rect r = hGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default];
                mHexArr[i, j] = new Hex(x, y, z, hexSize, r, this, new Vector2(i, j));
				Vector3 worldPos = new Vector3(x, hGrid.getTerrain().SampleHeight(mHexArr[i, j].position + transform.position) + hGrid.offsetFromTerrain, z) + transform.position;
				mHexArr[i, j].worldPosition = worldPos;
            }
        }

        float n = Mathf.Sqrt(3.0f) / 4.0f;
        mHexVerts = new Vector3[]
        {
            new Vector3(-0.25f, 0, -n) * mHexSize,
            new Vector3(-0.5f, 0, 0) * mHexSize,
            new Vector3(-0.25f, 0, n) * mHexSize,
            new Vector3(0.25f, 0, n) * mHexSize,
            new Vector3(0.5f, 0, 0) * mHexSize,
            new Vector3(0.25f, 0, -n) * mHexSize
        };

        mTriangles = new int[]
        {
            0, 1, 2,
            2, 5, 0,
            2, 3, 5,
            3, 4, 5
        };

        mHexUVs = new Vector2[] 
        {
            new Vector2(0.25f, 0.0f),
            new Vector2(0.0f, 0.433013f),
            new Vector2(0.25f, 0.866025f),
            new Vector2(0.75f, 0.866025f),
            new Vector2(1.0f, 0.433013f),
            new Vector2(0.75f, 0.0f)
        };

        UpdateValidity();
        //UpdateHexHeights();
        RebuildMesh();
    }

    public void UpdateHexHeights() {
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                if(mHexArr[i, j].isValid())
                    mHexArr[i, j].position.y = hGrid.getTerrain().SampleHeight(mHexArr[i, j].position + transform.position) + hGrid.offsetFromTerrain;
            }
        }
    }

    public void UpdateValidity() {
        Rect r = hGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default];
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                if (!isPositionOffTerrain(mHexArr[i, j].position + transform.position)) {
                    if (!mHexArr[i, j].ignoreAutoValidate()) {
                        if (isValidHex(mHexArr[i, j].position + transform.position)) {
                            mHexArr[i, j].setValid(true, false);
                        } else {
                            mHexArr[i, j].setValid(false, false);
                        }
                        mHexArr[i, j].setUVRect(r);
                    }
                } else {
                    mHexArr[i, j].setValid(false, false);
                }
            }
        }
    }

    public bool isValidHex(Vector3 worldPos) {
        float maxVariance = hGrid.maxVariance;
        float sum = hGrid.getTerrain().SampleHeight(worldPos);
        Vector3[] heights = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            heights[i] = mHexVerts[i] + worldPos;
            heights[i].y = hGrid.getTerrain().SampleHeight(heights[i]);
            sum += heights[i].y;
        }
        float avg = sum / 7.0f;
        if (Mathf.Abs(hGrid.getTerrain().SampleHeight(worldPos) - avg) > maxVariance)
            return false;
        for (int i = 0; i < 6; i++) {
            if (Mathf.Abs(hGrid.getTerrain().SampleHeight(mHexVerts[i] + worldPos) - avg) > maxVariance)
                return false;
        }
        if (Mathf.Abs(getDiffFromCoplanar(heights[0], heights[1], heights[3], heights[4])) > hGrid.coplanarTolerance)
            return false;
        if (Mathf.Abs(getDiffFromCoplanar(heights[1], heights[2], heights[4], heights[5])) > hGrid.coplanarTolerance)
            return false;

        return true;
    }

    float getDiffFromCoplanar(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        return Vector3.Dot((v3 - v1), Vector3.Cross((v2 - v1), (v4 - v3)));
    }

    bool isPositionOffTerrain(Vector3 worldPos) {
        Terrain terr = hGrid.getTerrain();

        Vector3 minPoint = terr.transform.position;
        Vector3 maxPoint = terr.transform.position + terr.terrainData.size;

        if (worldPos.x > maxPoint.x || worldPos.x < minPoint.x)
            return true;
        if (worldPos.z > maxPoint.z || worldPos.z < minPoint.z)
            return true;
        return false;
    }

    public void RebuildMesh() {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int triOffset = 0;
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                // if the hex is ignoring autovalidate, we need to display it so the user knows
                if (mHexArr[i, j].isValid() || (mHexArr[i, j].ignoreAutoValidate() && mIsLevelEditor)) {
                    // build verts
                    for (int k = 0; k < 6; k++) {
                        Vector3 v = new Vector3();
                        v.x = mHexVerts[k].x + mHexArr[i, j].position.x;
                        v.y = 0;
                        v.z = mHexVerts[k].z + mHexArr[i, j].position.z;
                        v.y = hGrid.getTerrain().SampleHeight(v + transform.position) + hGrid.offsetFromTerrain;
                        verts.Add(v);
                    }
                    // build tris
                    for (int k = 0; k < 12; k++) {
                        tris.Add(mTriangles[k] + triOffset);
                    }
                    triOffset = verts.Count;
                    // build uvs
                    for (int k = 0; k < 6; k++) {
                        Rect r = mHexArr[i, j].getUVRect();
                        Vector2 transUV = new Vector2(mHexUVs[k].x, mHexUVs[k].y);
                        transUV.x *= r.width;
                        transUV.y *= r.height;
                        transUV.x += r.x;
                        transUV.y += r.y;
                        uvs.Add(transUV);
                    }
                }
            }
        }

        mMesh.Clear();
        mMesh.vertices = verts.ToArray();
        mMesh.SetTriangles(tris.ToArray(), 0);
        mMesh.SetUVs(0, uvs);
        
        mMesh.RecalculateBounds();
        mMesh.RecalculateNormals();

        mMeshFilter.mesh = mMesh;
       

    }

    /*public void SetHexUV(int x, int y, Rect rect) {
        Vector2[] uvs = mMesh.uv;

        int uvIndex = 0;
        bool stop = false;
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                if (i == x && y == j) {
                    stop = true;
                    break;
                }
                if (mHexArr[i, j].isValid()) {
                    uvIndex += 6;
                }
            }
            if (stop)
                break;
        }
        for (int k = 0; k < 6; k++) {
            if (uvIndex + k >= uvs.GetLength(0))
                return;
            Vector2 transUV = new Vector2(mHexUVs[k].x, mHexUVs[k].y);
            transUV.x *= rect.width;
            transUV.y *= rect.height;
            transUV.x += rect.x;
            transUV.y += rect.y;
            uvs[uvIndex + k] = transUV;
        }
        mHexArr[x, y].setUVRect(rect);
        mMesh.uv = uvs;
        mMesh.UploadMeshData(false);

    }*/

    public void SetHexUVRebuild(int x, int y, Rect rect) {
        mHexArr[x, y].setUVRect(rect);
        RebuildMesh();
    }

    public void SetHexValid(int x, int y, bool isValid, bool ignoreAutoValidation) {
        mHexArr[x, y].setValid(isValid, ignoreAutoValidation);
    }

	/// <summary>
	/// This function should be called from the Terrain HexGrid only since it will have the index for the hex.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Hex getHex(Vector2 index) {
		int hexX = (int)index.x % hGrid.hexChunkSize;
		int hexY = (int)index.y % hGrid.hexChunkSize;
		if(hexX < 0 || hexY < 0) {
			return null;
		}
		return mHexArr[hexX, hexY];
	}

	/// <summary>
	/// Get the hexes that surround the hex specified
	/// </summary>
	/// <param name="hex">The Hex to check around</param>
	/// <param name="radius">The radius around this hex to check</param>
	/// <param name="validHexesOnly">Only return valid hexes</param>
	/// <returns></returns>
	public List<Hex> getSurroundingHexes(Hex hex, int radius, bool validHexesOnly) {
		List<Hex> hexes = new List<Hex>();

		return hexes;
	}

	/// <summary>
	/// Gets the index of this chunk into the TerrainHexGrid.
	/// Mainly to be used by the individual hexes when perfroming calculations
	/// </summary>
	/// <returns></returns>
	public Vector2 getIndexIntoGrid() {
		return mIndexIntoGrid;
	}

	public int getChunkSize() {
		return mChunkSize;
	}

    public void Save(FileStream stream) {
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                mHexArr[i, j].Save(stream);
            }
        }
    }

    public void Load(FileStream stream, bool isLevelEditor) {
        mIsLevelEditor = isLevelEditor;
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                mHexArr[i, j].Load(stream, isLevelEditor);
            }
        }
    }
}
