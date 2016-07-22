using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hex{

    public Vector3 position; // position of the lowerleft corner of the hex (Y is equal to chunk Y)

	public Vector3 worldPosition { get; set; }
	//Stored as even-q vertical coordinates
	private Vector2 mHexChunkIndex;
    private float mSize;
    private bool mValid;
    private bool mIgnoreAutoValidation;
    private Rect mUVRect;
	private HexChunk hChunk;

	/// <summary>
	/// Index order: Bottom right, top Right, top, top left, bottom left, bottom
	/// </summary>
	public static Vector3[] cubeDirections = new Vector3[] { new Vector3(1, -1, 0), new Vector3(1, 0, -1), new Vector3(0, 1, -1),
											new Vector3(-1, 1, 0), new Vector3(-1, 0, 1), new Vector3(0, -1, 1)};

    public Hex(float x, float y, float z, float size, Rect uvRect, HexChunk chunk, Vector2 chunkIndex) {
        position.x = x;
        position.y = y;
        position.z = z;
        mSize = size;
        mUVRect = uvRect;
		hChunk = chunk;
		mHexChunkIndex = chunkIndex;
    }

    public void setValid(bool isValid, bool ignoreAutoValidation) {
        mValid = isValid;
        mIgnoreAutoValidation = ignoreAutoValidation;
    }

    public bool isValid() {
        return mValid;
    }

    public bool ignoreAutoValidate() {
        return mIgnoreAutoValidation;
    }

    public void setUVRect(Rect r) {
        mUVRect = r;
    }

    public Rect getUVRect() {
        return mUVRect;
    }

	public HexChunk getChunk() {
		return hChunk;
	}

	public Vector2 getHexChunkIndex() {
		return mHexChunkIndex;
	}

	/// <summary>
	/// returns the Hex's cube coordinates relative to the entire grid
	/// </summary>
	/// <returns></returns>
	public Vector3 getCubeCoords() {
		Vector3 cubeCoords = Vector3.zero;
		int col = (int)(mHexChunkIndex.x + hChunk.getIndexIntoGrid().x * hChunk.getChunkSize());
		int row = (int)(mHexChunkIndex.y + hChunk.getIndexIntoGrid().y * hChunk.getChunkSize());
		cubeCoords.x = col;
		cubeCoords.z = row - (col - (col & 1)) / 2;
		cubeCoords.y = -cubeCoords.x - cubeCoords.z;
		return cubeCoords;
	}

	/// <summary>
	/// Get cube coords from even-q vertical offset coords
	/// </summary>
	/// <param name="evenQVertCoords">X: column, Y: row</param>
	/// <returns></returns>
	public static Vector3 getCubeCoordsFromOffset(Vector2 evenQVertCoords) {
		int col = (int)evenQVertCoords.x;
		int row = (int)evenQVertCoords.y;
		Vector3 cubeCoords = Vector3.zero;
		cubeCoords.x = col;
		cubeCoords.z = row - (col - (col & 1)) / 2;
		cubeCoords.y = -cubeCoords.x - cubeCoords.z;
		return cubeCoords;
	}

	/// <summary>
	/// Get even-q vertical offset coords from cube coords
	/// </summary>
	/// <param name="cubeCoords"></param>
	/// <returns>col, row</returns>
	public static Vector2 getEvenQVerticalCoords(Vector3 cubeCoords) {
		int col = (int)cubeCoords.x;
		int row = (int)(cubeCoords.z) + (col - (col & 1)) / 2;
		return new Vector2(col, row);
	}

	/// <summary>
	/// Basically just adds the two Vector3 objects together
	/// </summary>
	/// <param name="cubeCoord1"></param>
	/// <param name="cubeCoord2"></param>
	/// <returns></returns>
	public static Vector3 cubeAdd(Vector3 cubeCoord1, Vector3 cubeCoord2) {
		return cubeCoord1 + cubeCoord2;
	}

	public static Vector3 cubeNeighbor(Vector3 cubeCoord, int directionIndex) {
		return cubeCoord + cubeDirections[directionIndex];
	}
	
}
