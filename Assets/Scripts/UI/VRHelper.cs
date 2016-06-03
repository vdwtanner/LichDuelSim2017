using UnityEngine;
using System.Collections;

public class VRHelper : MonoBehaviour {
    public GameObject godBody { get; set; }
    public bool isTeleporting { get; set; }
    public Terrain terrain { get; set; }
    public float terrainHeight { get; set; }
    public GameObject possessionBody { get; set; }
    public Transform dominantHand { get; private set; }
    public Transform offHand { get; private set; }

    // Use this for initialization
    void Start () {
        godBody = transform.parent.gameObject;
        terrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();
        terrainHeight = terrain.transform.position.y;
        dominantHand = transform.FindChild(Config.dominantHand);
        offHand = transform.FindChild(Config.offHand);
    }
}
