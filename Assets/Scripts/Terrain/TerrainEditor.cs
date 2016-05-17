using UnityEngine;
using System.Collections;

/*
    Attach this script to a camera or gameobject to give it terrain editing tools at runtime.
*/
public class TerrainEditor : MonoBehaviour {

    [Header("Brush Settings")]
    [Range(0, 100)] public int size;
    [Range(0.0f, 1.0f)] public float opacity;

    [Header("Add/Remove Height Tool")]
    public char addRemoveKey;

    [Header("Paint Height Tool")]
    public char paintHeightKey;

    [Header("Smooth Tool")]
    public char smoothKey;

    private AddRemoveHeightTool mAddRemoveTool;
    private PaintHeightTool mPaintHeightTool;
    private SmoothHeightTool mSmoothTool;

	// Use this for initialization
	void Start () {
        // add all tool scripts to the object we're on
        mAddRemoveTool = gameObject.AddComponent<AddRemoveHeightTool>();
        mPaintHeightTool = gameObject.AddComponent<PaintHeightTool>();
        mSmoothTool = gameObject.AddComponent<SmoothHeightTool>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
