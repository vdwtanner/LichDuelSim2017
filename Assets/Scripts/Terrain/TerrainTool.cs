using UnityEngine;
using System.Collections;

public abstract class TerrainTool : MonoBehaviour {

    protected TerrainEditor mTEditor;
    protected Terrain mTargetTerrain;
    protected RaycastHit mHit;

	// Use this for initialization
	protected void Initialize () {
        mTEditor = gameObject.GetComponent<TerrainEditor>();
	}
	
	// Update is called once per frame
	protected void FixedUpdate () {
        Physics.Raycast(transform.position, transform.forward, out mHit, 100);
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
