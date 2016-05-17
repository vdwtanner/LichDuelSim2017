using UnityEngine;
using System.Collections;

public abstract class TerrainTool : MonoBehaviour {

    public GameObject cursor;
    protected TerrainEditor mTEditor;

	// Use this for initialization
	void Start () {
        mTEditor = gameObject.GetComponent<TerrainEditor>();
        cursor = (GameObject)Instantiate(cursor, new Vector3(0, 0, 0), Quaternion.identity);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 100);
        if (hit.collider != null) {
            Terrain hitTerrain = hit.collider.gameObject.GetComponent<Terrain>();
            if (hitTerrain != null) {

            }
        }

	}


}
