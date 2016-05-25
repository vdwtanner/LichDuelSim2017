using UnityEngine;
using System.Collections;

public class VRHelper : MonoBehaviour {
    public GameObject godBody { get; set; }
    public bool isTeleporting { get; set; }
    public Terrain terrain { get; set; }
    public float terrainHeight { get; set; }
    public GameObject possessionBody { get; set; }

    // Use this for initialization
    void Start () {
        godBody = transform.parent.gameObject;
        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();
        terrainHeight = terrain.transform.position.y;
    }
}
