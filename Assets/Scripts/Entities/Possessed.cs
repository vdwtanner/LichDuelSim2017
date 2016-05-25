using UnityEngine;
using System.Collections;

public class Possessed : MonoBehaviour {
    private Vector3 hmdOffset;
    private Transform headTransform;
	// Use this for initialization
	void Start () {
        hmdOffset = transform.FindChild("HMD").localPosition;
        hmdOffset.x *= transform.lossyScale.x;
        hmdOffset.y *= transform.lossyScale.y;
        hmdOffset.z *= transform.lossyScale.z;
    }
	
	// want to move model with player
	void FixedUpdate () {
        Vector3 pos = headTransform.position;
        pos.y = transform.position.y;
        transform.position = pos;
	}

    public void setHeadTransform(Transform head) {
        headTransform = head;
    }
}
