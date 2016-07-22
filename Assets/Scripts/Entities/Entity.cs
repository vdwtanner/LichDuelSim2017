using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Entity : MonoBehaviour {
	private Hex mHex;
	public bool m_snapToGrid;
	private bool mSnapRequested;
	private bool mOnTerrain = false;
	private bool mShowMovementRange;
	public int m_team = 0;

	private UnitStats hUnitStats;

	//Movement Range
	private HashSet<Hex> mMovementRange;
	private HashSet<HexChunk> mMovementChunks;

	private TerrainHexGrid h_terrainHexGrid;

	void Start() {
		hUnitStats = GetComponent<UnitStats>();
		h_terrainHexGrid = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainHexGrid>();
		if (h_terrainHexGrid == null) {
			Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
			throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
		}
		/*Hex hex = thg.getHexFromWorldPos(transform.position);
		if (hex.isValid()) {
			snapTo(hex.worldPosition);
			mHex = hex;
		}*/
	}

	public void showMovementRange() {
		if(hUnitStats != null) {
			//TerrainHexGrid thg = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainHexGrid>();
			
			if (mHex == null) {
				setHex(h_terrainHexGrid.getHexFromWorldPos(transform.position));
			}
			mMovementRange = h_terrainHexGrid.getReachableHexes(mHex, hUnitStats.m_movementSpeed, m_team);
			mMovementChunks = new HashSet<HexChunk>();
			foreach(Hex hex in mMovementRange) {
				if(hex.getEntity() == null) {
					hex.setUVRect(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Valid]);
					if (!mMovementChunks.Contains(hex.getChunk())) {
						mMovementChunks.Add(hex.getChunk());
					}
				}else if(hex == mHex) {
					hex.setUVRect(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.CurrentPos]);
					if (!mMovementChunks.Contains(hex.getChunk())) {
						mMovementChunks.Add(hex.getChunk());
					}
				}
				
			}
			foreach (HexChunk chunk in mMovementChunks) {
				chunk.RebuildMesh();
			}
		}
		
	}

	public void snapToGrid() {
		//Debug.Log("Snap to grid called");
		if (m_snapToGrid) {
			mSnapRequested = true;
			//Debug.Log("requesting to snap");
			if (mOnTerrain) {
				//TODO use a faster implementation of this
				Hex hex = h_terrainHexGrid.getHexFromWorldPos(transform.position);
				if (hex.isValid()) {
					snapTo(hex.worldPosition);
					setHex(hex);
					mSnapRequested = false;
					if(mMovementRange != null) {
						foreach (Hex h in mMovementRange) {
							h.setUVRect(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default]);
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
			Hex hex = h_terrainHexGrid.getHexFromWorldPos(transform.position);
			if (hex.isValid() && mMovementRange.Contains(hex)) {
				snapTo(hex.worldPosition);
				setHex(hex);
				if (mMovementRange != null) {
					foreach (Hex h in mMovementRange) {
						h.setUVRect(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default]);
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

	private void setHex(Hex hex) {
		if (mHex != null) {
			mHex.setEntity(null);
		}
		hex.setEntity(this);
		mHex = hex;
	}

	public void showSelectionHex() {
		if(mHex != null) {
			mHex.setUVRectAndUpdate(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.CurrentPos]);
		}
	}

	public void removeSelectionHex() {
		if (mHex != null) {
			mHex.setUVRectAndUpdate(h_terrainHexGrid.getAtlas()[(int)TerrainHexGrid.HexTextureType.Default]);
		}
	}
}
