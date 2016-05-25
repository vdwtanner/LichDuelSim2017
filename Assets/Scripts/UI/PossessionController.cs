using UnityEngine;
using System.Collections;

public class PossessionController : MonoBehaviour {
    Controller controller;
    VRHelper cameraRig;

    //Teleporting
    public Color teleportRayColor;
    public float teleportDistance = 20;
    public ParticleSystem ps;
    public LayerMask teleportMask;
    private bool teleportPrep = false;


    GameObject hmd;

    // Use this for initialization
    void Start () {
        controller = gameObject.GetComponent<Controller>();
        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
        hmd = transform.parent.FindChild("Camera (eye)").gameObject;
        cameraRig = transform.parent.GetComponent<VRHelper>();
    }
	
	// Update is called once per frame
	void Update () {
        teleportationManager();
        if (controller.getButtonDown("menu")) {
            Vector3 pos = transform.parent.parent.position;
            pos.y = (cameraRig.terrain.SampleHeight(pos) + cameraRig.terrainHeight) - Config.HMDStadningHeight / 2.0f + Config.godTeleportationHeightOffset;
            cameraRig.godBody.transform.position = pos;
            transform.parent.parent = cameraRig.godBody.transform;
            transform.parent.localPosition = Vector3.zero;
            //transform.parent.parent.position = pos;
            transform.parent.localScale = new Vector3(Config.godScale, Config.godScale, Config.godScale);
            Destroy(cameraRig.possessionBody.GetComponent<Possessed>());
            transform.parent.BroadcastMessage("switchControl", "god");
        }
    }

    void teleportationManager() {
        if (controller.getButtonPressed("touchpad")) {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            Debug.DrawRay(transform.position, forward * 200, Color.green);
        }
        if (controller.getButtonPressed("touchpad") && (!cameraRig.isTeleporting || teleportPrep)) {
            //tce.transform.position = transform.position;
            teleportPrep = true;
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            RaycastHit hit;

            Ray ray = new Ray(transform.position, forward);
            if (Physics.Raycast(ray, out hit, teleportDistance, teleportMask)) {
                ps.transform.position = hit.point;
                if (!cameraRig.isTeleporting) {
                    ParticleSystem.EmissionModule em = ps.emission;
                    em.enabled = true;
                    ps.Play();
                    cameraRig.isTeleporting = true;
                    Debug.Log(transform.name + " controller began teleport particle emmission.");
                }
            }
        } else if (controller.getButtonUp("touchpad") && teleportPrep) {
            ps.Stop();
            cameraRig.isTeleporting = false;
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            teleportPrep = false;
            Debug.Log(transform.name + " controller stopped particle emmission.");
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
