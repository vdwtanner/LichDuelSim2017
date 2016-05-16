using UnityEngine;
using System.Collections;

public class GodController : MonoBehaviour {

    Controller controller;
    public Color teleportRayColor;
    public float teleportDistance = 200;
    public ParticleSystem ps;
    private bool iOwnPS = false;
    public LayerMask teleportMask;
    public GameObject teleportationChargeEffect;
    private GameObject tce;
    
	// Use this for initialization
	void Start () {
        controller = gameObject.GetComponent<Controller>();
    }
	
	// Update is called once per frame
	void Update () {
        if (controller.getButtonDown("touchpad") && (ps.isStopped || iOwnPS)) {
            tce = (GameObject)Instantiate(teleportationChargeEffect, transform.position, new Quaternion());
            tce.GetComponent<ParticleSystem>().Play();
        }
        if (controller.getButtonPressed("touchpad") && (ps.isStopped || iOwnPS)) {
            //ps.Play();
            if (tce != null) {
                tce.transform.position = transform.position;
            }
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            RaycastHit hit;
            Debug.DrawRay(transform.position, forward * 200, Color.green);
            Ray ray = new Ray(transform.position, forward);
            if(Physics.Raycast(ray, out hit, teleportDistance, teleportMask)) {
                ps.transform.position = hit.point;
                ps.Emit(1);
                //ps.Play();
                iOwnPS = true;
            }
        }else if (controller.getButtonUp("touchpad") && iOwnPS) {
            ps.Stop();
            tce.GetComponent<ParticleSystem>().Stop();
            iOwnPS = false;
            transform.parent.parent.position = ps.transform.position;
        }
	}
}
