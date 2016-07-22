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
        Invalid = 2,
        Attack = 3,
		CurrentPos = 4
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
    public Texture invalidTex;
	public Texture currentPositionTex;


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


        Texture2D[] texArray = new Texture2D[5];
        texArray[(int)HexTextureType.Default] = defaultTex as Texture2D;
        texArray[(int)HexTextureType.Valid] = validMoveTex as Texture2D;
        texArray[(int)HexTextureType.Attack] = attackRangeTex as Texture2D;
        texArray[(int)HexTextureType.Invalid] = invalidTex as Texture2D;
		texArray[(int)HexTextureType.CurrentPos] = currentPositionTex as Texture2D;

		mAtlasTexture = new Texture2D(1048, 1048);
        mAtlas = mAtlasTexture.PackTextures(texArray, 2, 1048);

        hexMaterial.SetTexture("_MainTex", mAtlasTexture);

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
                chunk.Initialize(this, pos, hexChunkSize, hexSize, hexMaterial, new Vector2(i, j));
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


    Vector2 GetHexIndexFromWorldPos(Vector3 worldPos) {
        Vector3 gridlocal = worldPos - (transform.position - new Vector3(mHexSideLength/2, 0, mHexEdgeToEdgeLength/2));

        // x position
        // http://stackoverflow.com/questions/7705228/hexagonal-grids-how-do-you-find-which-hexagon-a-point-is-in
        // using the box based solution described above,
        // 0.75 is the gridHeight of a size 1 hex
        float boxX = gridlocal.x / (0.75f * hexSize);

        // y position
        float boxY = 0;
        if ((int)boxX % 2 == 0) {
            boxY = (gridlocal.z / mHexEdgeToEdgeLength);
        } else {
            boxY = ((gridlocal.z - (mHexEdgeToEdgeLength / 2)) / mHexEdgeToEdgeLength);
        }

        float relX = boxX - (int)(boxX);
        float relY = boxY - (int)(boxY);

        float m = (mHexEdgeToEdgeLength / 2) / (.25f * hexSize);

        float cposM = 0.8660254f;
        float cnegM = cposM + mHexEdgeToEdgeLength;

        // the constants are arbitrary, my c values are wrong but could never figure out why. The extra 0.35 and 0.5 seem to get it accurate enough
        if (relY < (m * relX) - cposM - 0.35f) {
            if ((int)boxX % 2 == 0)
                boxY--;
            boxX++;
        } else if (relY > (-m * relX) + cnegM + 0.5f) {
            if ((int)boxX % 2 == 1)
                boxY++;
            boxX++;
        }

        return new Vector2((int)boxX, (int)boxY);
    }

	public Hex getHexFromWorldPos(Vector3 worldPos) {
		Vector2 idx = GetHexIndexFromWorldPos(worldPos);
		int chunkIdxX = (int)idx.x / hexChunkSize;
		int chunkIdxY = (int)idx.y / hexChunkSize;
		return mChunkList[chunkIdxX, chunkIdxY].getHex(idx);
	}

    public void SetHexTexture(Vector3 worldPos, HexTextureType texture) {
        Vector2 idx = GetHexIndexFromWorldPos(worldPos);
        int chunkIdxX = (int)idx.x / hexChunkSize;
        int chunkIdxY = (int)idx.y / hexChunkSize;
        mChunkList[chunkIdxX, chunkIdxY].SetHexUVRebuild((int)idx.x % hexChunkSize, (int)idx.y % hexChunkSize, mAtlas[(int)texture]);
    }

    public void SetHexValid(Vector3 worldPos, bool isValid, bool ignoreAutoValidation = false) {
        Vector2 idx = GetHexIndexFromWorldPos(worldPos);
        int chunkIdxX = (int)idx.x / hexChunkSize;
        int chunkIdxY = (int)idx.y / hexChunkSize;
        mChunkList[chunkIdxX, chunkIdxY].SetHexValid((int)idx.x % hexChunkSize, (int)idx.y % hexChunkSize, isValid, ignoreAutoValidation);
        if(isValid)
            mChunkList[chunkIdxX, chunkIdxY].SetHexUVRebuild((int)idx.x % hexChunkSize, (int)idx.y % hexChunkSize, mAtlas[(int)HexTextureType.Valid]);
        else if (!isValid && ignoreAutoValidation)
            mChunkList[chunkIdxX, chunkIdxY].SetHexUVRebuild((int)idx.x % hexChunkSize, (int)idx.y % hexChunkSize, mAtlas[(int)HexTextureType.Invalid]);
    }

    public Terrain getTerrain() {
        return hTerrain;
    }

    public Texture2D getAtlasTexture() {
        return mAtlasTexture;
    }

    public Rect[] getAtlas() {
        return mAtlas;
    }

	/// <summary>
	/// Get all Hexes reachable from the specified cube coord.
	/// </summary>
	/// <param name="cubeCoord"></param>
	/// <param name="range"></param>
	/// <returns></returns>
	public HashSet<Hex> getReachableHexes(Hex hex, int range) {
		Vector3 cubeCoord = hex.getCubeCoords();
		HashSet<Hex> visited = new HashSet<Hex>();
		visited.Add(hex);
		List<Vector3> fringe = new List<Vector3>();
		fringe.Add(cubeCoord);
		for (int i = 1; i <= range; i++) {
			List<Vector3> nextFringe = new List<Vector3>();
			foreach (Vector3 cube in fringe) {
				for (int x = 0; x < 6; x++) {
					Vector3 neighborCube = Hex.cubeNeighbor(cube, x);
					Vector2 neighborOffset = Hex.getOddQVerticalCoords(neighborCube);
					Hex neighbor = getHexFromOffsetCoords(neighborOffset);
					if (neighbor != null && !visited.Contains(neighbor) && neighbor.isValid()) {
						visited.Add(neighbor);
						nextFringe.Add(neighborCube);
					}
				}
			}
			fringe = nextFringe;
		}
		return visited;
	}

	/// <summary>
	/// Get all Hexes reachable from the specified cube coord.
	/// Denies movement through hexes that contain Entities associated with a different team.
	/// </summary>
	/// <param name="hex"></param>
	/// <param name="range"></param>
	/// <param name="team">The team to compare against</param>
	/// <returns></returns>
	public HashSet<Hex> getReachableHexes(Hex hex, int range, int team) {
		Vector3 cubeCoord = hex.getCubeCoords();
		HashSet<Hex> visited = new HashSet<Hex>();
		visited.Add(hex);
		List<Vector3> fringe = new List<Vector3>();
		fringe.Add(cubeCoord);
		for (int i = 1; i <= range; i++) {
			List<Vector3> nextFringe = new List<Vector3>();
			foreach (Vector3 cube in fringe) {
				for (int x = 0; x < 6; x++) {
					Vector3 neighborCube = Hex.cubeNeighbor(cube, x);
					Vector2 neighborOffset = Hex.getOddQVerticalCoords(neighborCube);
					Hex neighbor = getHexFromOffsetCoords(neighborOffset);
					if (neighbor != null && !visited.Contains(neighbor) && neighbor.isValid()) {
						Entity entity = neighbor.getEntity();
						if(entity != null) {
							if(entity.m_team == team) {
								visited.Add(neighbor);
								nextFringe.Add(neighborCube);
							}
						} else {
							visited.Add(neighbor);
							nextFringe.Add(neighborCube);
						}
						
					}
				}
			}
			fringe = nextFringe;
		}
		return visited;
	}

	public Hex getHexFromOffsetCoords(Vector2 coords) {
		int x = (int)coords.x;//col
		int z = (int)coords.y;//row
		HexChunk chunk = mChunkList[x / hexChunkSize, z / hexChunkSize];
		return chunk.getHex(new Vector2(x, z));
	}
}
