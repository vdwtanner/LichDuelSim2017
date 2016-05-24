using UnityEngine;
using System.Collections;

public abstract class TerrainTool{

    protected TerrainEditor mTEditor;
    protected Terrain mTargetTerrain;
    protected RaycastHit mHit;

	// Use this for initialization
	public void Initialize (TerrainEditor editor) {
        mTEditor = editor.gameObject.GetComponent<TerrainEditor>();
	}
	
	public void FixedUpdate () {
        Physics.Raycast(mTEditor.transform.position, mTEditor.transform.forward, out mHit, 100);
        if (mHit.collider != null) {
            Terrain hitTerrain = mHit.collider.gameObject.GetComponent<Terrain>();
            if (hitTerrain != null) {
                mTargetTerrain = hitTerrain;
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

    public RaycastHit getHit() {
        return mHit;
    }

    public abstract void ModifyTerrain();


}
