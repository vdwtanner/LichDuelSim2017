using UnityEngine;
using System.Collections;

public abstract class EditorTool{

    protected TerrainEditor mTEditor;
    protected Terrain mTargetTerrain;
    protected TerrainHexGrid mTargetHexGrid;
    protected RaycastHit mHit;
    protected Controller hController;

	// Use this for initialization
	public void Initialize (TerrainEditor editor) {
        mTEditor = editor.gameObject.GetComponent<TerrainEditor>();
        hController = editor.gameObject.GetComponent<Controller>();
	}
	
	public void FixedUpdate () {
        Physics.Raycast(mTEditor.transform.position, mTEditor.transform.forward, out mHit, 100);
        if (mHit.collider != null) {
            Terrain hitTerrain = mHit.collider.gameObject.GetComponent<Terrain>();
            if (hitTerrain != null) {
                mTargetTerrain = hitTerrain;
                mTargetHexGrid = hitTerrain.GetComponent<TerrainHexGrid>();
                mTEditor.getCursor().transform.position = new Vector3(mHit.point.x, mHit.point.y + 10f, mHit.point.z);
                mTEditor.getCursor().GetComponent<Projector>().orthographicSize = (mTEditor.getBrushSize() / 2) * hitTerrain.terrainData.heightmapScale.x;
            }
        }
	}

    protected TerrainEditor getEditor() {
        return mTEditor;
    }

    public Terrain getHitTerrain() {
        return mTargetTerrain;
    }

    public TerrainHexGrid getHexGrid() {
        return mTargetHexGrid;
    }

    public RaycastHit getHit() {
        return mHit;
    }

    public abstract void ModifyTerrain();
    public abstract void BrushAltFire();
    /// <summary>
    /// This is called on the frame that the alt fire button is released
    /// </summary>
    public abstract void BrushAltFireUp();
    public abstract void OnSelection();

}
