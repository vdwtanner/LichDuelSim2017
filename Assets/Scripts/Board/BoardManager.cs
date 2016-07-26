using UnityEngine;
using System.Collections;
using System.IO;

// The board manager's primary function is to save and load all data for the terrainmanager,
// unit manager, and hex grid. Effectively, the board manager handles the header information
// in the file, while the other managers handle raw data
public class BoardManager : MonoBehaviour {

    public bool isLevelEditor;

	protected TerrainManager mTerrainManager;
    protected TerrainHexGrid mHexGrid;

	// Use this for initialization
	void Start () {
        mTerrainManager = GetComponentInChildren<TerrainManager>();
        if (mTerrainManager == null)
            Debug.LogError("Board has no terrain!");
        mHexGrid = GetComponentInChildren<TerrainHexGrid>();
        if (mHexGrid == null)
            Debug.LogError("Board has no hex grid!");


	}
	
	// Update is called once per frame
	void Update () {
        bool save = Input.GetKeyUp(KeyCode.K);
        bool load = Input.GetKeyUp(KeyCode.L);

        if (save)
            Save("ExampleBoard.godmap");
        if (load)
            Load("ExampleBoard.godmap");
	}

	public void Save(string filename) {
        FileStream file = new FileStream(Application.dataPath + "/" + filename, FileMode.OpenOrCreate, FileAccess.Write);
        
        mTerrainManager.Save(file);

        mHexGrid.Save(file);

		file.Close();
	}

	public void Load(string filename) {
        FileStream file = new FileStream(Application.dataPath + "/" + filename, FileMode.Open, FileAccess.Read);

        mTerrainManager.Load(file);

        mHexGrid.Load(file, isLevelEditor);

        file.Close();
    }
}
