using UnityEngine;
using System.Collections;

public class LevelEditorController : MonoBehaviour {
    public GameObject teleportationMarker;
    Controller controller;
    GameObject hmd;
    VRHelper cameraRig;

    //Teleporting
    public Color teleportRayColor;
    public float teleportDistance = 800;
    private ParticleSystem ps;
    private bool iOwnPS = false;
    public LayerMask teleportMask;

    // Use this for initialization
    void Start () {
        controller = gameObject.GetComponent<Controller>();
        teleportationMarker = Instantiate(teleportationMarker);
        ps = teleportationMarker.GetComponent<ParticleSystem>();
        if(ps == null) {
            Debug.LogError("TeleportationMarker must have a ParticleSystem Component");
            Application.Quit();
        }
        hmd = transform.parent.FindChild("Camera (eye)").gameObject;
        cameraRig = transform.parent.GetComponent<VRHelper>();
    }
	
	// Update is called once per frame
	void Update () {
        teleportationManager();
        calibrationManager();
    }

    void teleportationManager() {
        if (controller.getButtonPressed("touchpad")) {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            //Debug.DrawRay(transform.position, forward * 200, Color.green);
        }
        if (controller.getButtonPressed("touchpad") && (!cameraRig.isTeleporting || iOwnPS)) {
            //tce.transform.position = transform.position;
            /*if (!tce.isPlaying) {
                tce.Play();
                Debug.Log(transform.name + " controller began particle emmission.");
            }*/
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
                    //Debug.Log(transform.name + " controller began teleport particle emmission.");
                }

                //ps.Emit(1);
                //ps.Play();
                iOwnPS = true;
            }
        } else if (controller.getButtonUp("touchpad") && iOwnPS) {
            ps.Stop();
            cameraRig.isTeleporting = false;
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            //tce.Stop();
            //Debug.Log(transform.name + " controller stopped particle emmission.");
            iOwnPS = false;
            Vector3 pos = ps.transform.position;
            pos.y = cameraRig.terrain.SampleHeight(pos) + cameraRig.terrainHeight;
            //Instantiate(marker).transform.position = pos;
            pos.y = (cameraRig.terrain.SampleHeight(pos) + cameraRig.terrainHeight) - Config.HMDStadningHeight / 2.0f + Config.godTeleportationHeightOffset;
            Vector3 offset = hmd.transform.localPosition;
            offset.y = 0;
            pos -= offset * transform.parent.localScale.x;
            transform.parent.parent.position = pos;
        }
    }

    void calibrationManager() {
        if (controller.getButtonDown("menu")) {
            Transform hmd = transform.parent.FindChild("Camera (eye)").transform;
            float scale = transform.parent.lossyScale.y;
            Config.HMDStadningHeight = hmd.localPosition.y * scale;
            Debug.Log("Height Calibrated to " + Config.HMDStadningHeight);
        }
    }
}
