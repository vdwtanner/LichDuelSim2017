using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Entity : MonoBehaviour {
	private Hex mHex;
	public bool mSnapToGrid;
	private bool mSnapRequested;
	private bool mOnTerrain = false;
	private bool mShowMovementRange;

	private UnitStats hUnitStats;

	//Movement Range
	private HashSet<Hex> mMovementRange;
	private HashSet<HexChunk> mMovementChunks;

	void Start() {
		hUnitStats = GetComponent<UnitStats>();
	}

	public void showMovementRange() {
		if(hUnitStats != null) {
			TerrainHexGrid thg = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainHexGrid>();
			if (thg == null) {
				Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
				throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
			}
			if (mHex == null) {
				mHex = thg.getHexFromWorldPos(transform.position);
			}
			mMovementRange = thg.getReachableHexes(mHex, hUnitStats.mMovementSpeed);
			mMovementChunks = new HashSet<HexChunk>();
			foreach(Hex hex in mMovementRange) {
				hex.setUVRect(thg.getAtlas()[(int)TerrainHexGrid.HexTextureType.Valid]);
				if (!mMovementChunks.Contains(hex.getChunk())) {
					mMovementChunks.Add(hex.getChunk());
				}
			}
			foreach (HexChunk chunk in mMovementChunks) {
				chunk.RebuildMesh();
			}
		}
		
	}

	public void snapToGrid() {
		//Debug.Log("Snap to grid called");
		if (mSnapToGrid) {
			mSnapRequested = true;
			//Debug.Log("requesting to snap");
			if (mOnTerrain) {
				//TODO use a faster implementation of this
				TerrainHexGrid thg = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainHexGrid>();
				if (thg == null) {
					Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
					throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
				}
				Hex hex = thg.getHexFromWorldPos(transform.position);
				if (hex.isValid()) {
					snapTo(hex.worldPosition);
					mHex = hex;
					mSnapRequested = false;
					if(mMovementRange != null) {
						foreach (Hex h in mMovementRange) {
							h.setUVRect(thg.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default]);
						}
						foreach(HexChunk chunk in mMovementChunks) {
							chunk.RebuildMesh();
						}
					}
					
				} else {
					Debug.LogWarning("Invalid snap location");
				}
			}
			
		}
	}

	private void snapTo(Vector3 pos) {
		transform.position = pos;
		Vector3 rot = new Vector3(0, transform.eulerAngles.y, 0);
		transform.eulerAngles = rot;
		Rigidbody body = GetComponent<Rigidbody>();
		if (body != null) {
			body.velocity = Vector3.zero;
			body.useGravity = false;
			//body.freezeRotation = true;
			body.constraints = RigidbodyConstraints.FreezeAll;
		}
	}

	void OnCollisionEnter(Collision collision) {
		Collider other = collision.collider;
		Terrain t = other.GetComponent<Terrain>();
		if (t != null) {
			mOnTerrain = true;
			if (!mSnapRequested) {
				return;
			}
			TerrainHexGrid thg = t.GetComponent<TerrainHexGrid>();
			if (thg == null) {
				Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
				throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
			}
			Hex hex = thg.getHexFromWorldPos(transform.position);
			if (hex.isValid() && mMovementRange.Contains(hex)) {
				snapTo(hex.worldPosition);
				mHex = hex;
				if (mMovementRange != null) {
					foreach (Hex h in mMovementRange) {
						h.setUVRect(thg.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default]);
					}
					foreach (HexChunk chunk in mMovementChunks) {
						chunk.RebuildMesh();
					}
				}
			} else {
				Debug.LogWarning("Invalid snap location");
				snapTo(mHex.worldPosition);
			}
		}
	}

	void OnCollisionExit(Collision collision) {
		Collider other = collision.collider;
		Terrain t = other.GetComponent<Terrain>();
		if (t != null) {
			mOnTerrain = false;
		}
	}

	public Hex getHex() {
		return mHex;
	}

	public void setHex(Hex hex) {
		mHex = hex;
	}
}
