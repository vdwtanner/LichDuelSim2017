using UnityEngine;
using System.Collections;

public class GodController : MonoBehaviour {

    Controller controller;

    //Teleporting
    public Color teleportRayColor;
    public float teleportDistance = 200;
    public ParticleSystem ps;
    private bool iOwnPS = false;
    public LayerMask teleportMask;
    public GameObject teleportationChargeEffect;
    //Teleport Charge Effect particle system
    private ParticleSystem tce;

    //Grabbing
    public GameObject objectToGrab {get; set;}

    
	// Use this for initialization
	void Start () {
        controller = gameObject.GetComponent<Controller>();
        GameObject go = (GameObject)Instantiate(teleportationChargeEffect, transform.position, new Quaternion());
        tce = go.GetComponent<ParticleSystem>();
        tce.Stop();
        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        teleportationManager();
        grabManager();
	}

    void teleportationManager() {
        if (controller.getButtonPressed("touchpad")) {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            Debug.DrawRay(transform.position, forward * 200, Color.green);
        }
        if (controller.getButtonPressed("touchpad") && (!ps.emission.enabled || tce.isPlaying)) {
            tce.transform.position = transform.position;
            if (!tce.isPlaying) {
                tce.Play();
                Debug.Log(transform.name + " controller began particle emmission.");
            }
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            RaycastHit hit;

            Ray ray = new Ray(transform.position, forward);
            if (Physics.Raycast(ray, out hit, teleportDistance, teleportMask)) {
                ps.transform.position = hit.point;
                if (!ps.isPlaying) {
                    ParticleSystem.EmissionModule em = ps.emission;
                    em.enabled = true;
                    ps.Play();
                    Debug.Log(transform.name + " controller began teleport particle emmission.");
                }

                //ps.Emit(1);
                //ps.Play();
                iOwnPS = true;
            }
        } else if (controller.getButtonUp("touchpad") && tce.isPlaying) {
            ps.Stop();
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            tce.Stop();
            Debug.Log(transform.name + " controller stopped particle emmission.");
            iOwnPS = false;
            transform.parent.parent.position = ps.transform.position;
        }
    }

    void grabManager() {
        if(objectToGrab != null) {
            if (controller.getButtonPressed("trigger")) {
                objectToGrab.transform.parent = transform;
            } else if (objectToGrab.transform.parent == transform) {
                objectToGrab.transform.parent = null;
            }
        }
    }
}
