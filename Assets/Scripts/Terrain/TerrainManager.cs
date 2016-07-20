using UnityEngine;
using System.Collections;

// The terrain manager saves and loads heightmap, splatmap, and other data for terrain rendering.
public class TerrainManager : MonoBehaviour {

    protected Terrain hTerrain;

    // Use this for initialization
    void Start () {
        hTerrain = GetComponentInChildren<Terrain>();
        if (hTerrain == null)
            Debug.LogError("TerrainManager has no Terrain!");
    }

    void Save() {

    }
}
