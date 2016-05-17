using UnityEngine;
using System.Collections;

public class PossessionController : MonoBehaviour {
    Controller controller;

    //Teleporting
    public Color teleportRayColor;
    public float teleportDistance = 200;
    public ParticleSystem ps;
    private bool iOwnPS = false;
    public LayerMask teleportMask;
    private bool teleportPrep = false;


    GameObject hmd;

    // Use this for initialization
    void Start () {
        controller = gameObject.GetComponent<Controller>();
        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
        hmd = transform.parent.FindChild("Camera (eye)").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        teleportationManager();
        if (controller.getButtonDown("menu")) {
            Vector3 pos = transform.parent.parent.position;
            pos.y = 0;
            transform.parent.parent.position = pos;
            transform.parent.localScale = new Vector3(Config.godScale, Config.godScale, Config.godScale);
            transform.parent.BroadcastMessage("switchControl", "god");
        }
    }

    void teleportationManager() {
        if (controller.getButtonPressed("touchpad")) {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            Debug.DrawRay(transform.position, forward * 200, Color.green);
        }
        if (controller.getButtonPressed("touchpad") && (!ps.emission.enabled || teleportPrep)) {
            //tce.transform.position = transform.position;
            teleportPrep = true;
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
        } else if (controller.getButtonUp("touchpad") && teleportPrep) {
            ps.Stop();
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            teleportPrep = false;
            Debug.Log(transform.name + " controller stopped particle emmission.");
            iOwnPS = false;
            Vector3 pos = ps.transform.position;
            Vector3 offset = hmd.transform.localPosition;
            offset.y = 0;
            pos -= offset * transform.parent.localScale.x;
            transform.parent.parent.position = pos;
        }
    }
    public void switchControl(string control) {
        switch (control) {
            case "possession":
                this.enabled = true;
                break;
            case "god":
                this.enabled = false;
                break;
        }
    }
}
