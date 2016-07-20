using UnityEngine;
using System.Collections;
using System.IO;

// The board manager's primary function is to save and load all data for the terrainmanager,
// unit manager, and hex grid. Effectively, the board manager handles the header information
// in the file, while the other managers handle raw data
public class BoardManager : MonoBehaviour {

	protected TerrainManager hTerrainManager;
    protected TerrainHexGrid hHexGrid;

	// Use this for initialization
	void Start () {
        hTerrainManager = GetComponentInChildren<TerrainManager>();
        if (hTerrainManager == null)
            Debug.LogError("Board has no terrain!");
        hHexGrid = GetComponentInChildren<TerrainHexGrid>();
        if (hHexGrid == null)
            Debug.LogError("Board has no hex grid!");


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Save(string filename) {
		FileStream file = File.Create(Application.persistentDataPath + "/" + filename);

		file.Close();
	}

	void Load(string filename) {
		
	}
}
