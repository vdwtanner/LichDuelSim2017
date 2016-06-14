using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexChunk : MonoBehaviour {

    float mHexSize;
    float mHexSideLength;
    float mHexEdgeToEdgeLength;
    int mChunkSize;

    Hex[,] mHexArr;

    private TerrainHexGrid hGrid;

    private Material mMaterial;
    private MeshRenderer mMeshRenderer;
    private MeshFilter mMeshFilter;
    private Mesh mMesh;

    private Vector3[] mHexVerts;
    private int[] mTriangles;
    private Vector2[] mHexUVs;

    public void Initialize(TerrainHexGrid grid, Vector3 chunkPos, int chunkSize, float hexSize, Material mat) {
        transform.position = chunkPos;
        mChunkSize = chunkSize;
        mHexSize = hexSize;

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
                mHexArr[i, j] = new Hex(x, y, z, hexSize, r);
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
                if (!mHexArr[i, j].ignoreAutoValidate()) {
                    if (isValidHex(mHexArr[i, j].position + transform.position)) {
                        mHexArr[i, j].setValid(true, false);
                    } else {
                        mHexArr[i, j].setValid(false, false);
                    }
                    mHexArr[i, j].setUVRect(r);
                }
            }
        }
    }

    public bool isValidHex(Vector3 pos) {
        float maxVariance = hGrid.maxVariance;
        float sum = hGrid.getTerrain().SampleHeight(pos);
        Vector3[] heights = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            heights[i] = mHexVerts[i] + pos;
            heights[i].y = hGrid.getTerrain().SampleHeight(heights[i]);
            sum += heights[i].y;
        }
        float avg = sum / 7.0f;
        if (Mathf.Abs(hGrid.getTerrain().SampleHeight(pos) - avg) > maxVariance)
            return false;
        for (int i = 0; i < 6; i++) {
            if (Mathf.Abs(hGrid.getTerrain().SampleHeight(mHexVerts[i] + pos) - avg) > maxVariance)
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

    public void RebuildMesh() {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int triOffset = 0;
        for (int i = 0; i < mChunkSize; i++) {
            for (int j = 0; j < mChunkSize; j++) {
                // if the hex is ignoring autovalidate, we need to display it so the user knows
                if (mHexArr[i, j].isValid() || mHexArr[i, j].ignoreAutoValidate()) {
                    // build verts
                    for (int k = 0; k < 6; k++) {
                        Vector3 v = new Vector3();
                        v.x = mHexVerts[k].x;
                        v.y = 0;
                        v.z = mHexVerts[k].z;
                        v.y = hGrid.getTerrain().SampleHeight(v + mHexArr[i, j].position + transform.position) + hGrid.offsetFromTerrain;
                        verts.Add(v + mHexArr[i, j].position);
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
}
