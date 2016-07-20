using UnityEngine;
using System.Collections;

public class GodController : MonoBehaviour {

    Controller controller;
    GameObject hmd;
    //Terrain terrain;
    VRHelper cameraRig;

    //Teleporting
    public Color teleportRayColor;
    public float teleportDistance = 800;
    public ParticleSystem ps;
    private bool iOwnPS = false;
    public LayerMask teleportMask;
    public GameObject teleportationChargeEffect;
    //Teleport Charge Effect particle system
    private ParticleSystem tce;
    //private float terrainHeight;

    public GameObject marker;

    //Grabbing
    public GameObject objectToGrab {get; set;}

    //Possession
    public GameObject objectToPossess { get; set; }
    public GameObject possesionEffect;
    //Possession Particle Effect
    private ParticleSystem pe;
    private float posessionTimer;
    public bool possessing { get; set; }


	// Use this for initialization
	void Start () {
        controller = gameObject.GetComponent<Controller>();
        hmd = transform.parent.FindChild("Camera (eye)").gameObject;
        GameObject go = (GameObject)Instantiate(teleportationChargeEffect, transform.position, new Quaternion());
        tce = go.GetComponent<ParticleSystem>();
        tce.transform.parent = transform;
        tce.Stop();
        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
        go = (GameObject)Instantiate(possesionEffect, transform.position, new Quaternion());
        pe = go.GetComponent<ParticleSystem>();
        pe.Stop();
        pe.transform.parent = transform;
        
        cameraRig = transform.parent.GetComponent<VRHelper>();
    }
	
	// Update is called once per frame
	void Update () {
        teleportationManager();
        grabManager();
        possessionManager();
        calibrationManager();
	}

    void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Terrain>() != null) {
            controller.hapticPulse(2200);
        }
    }

    void teleportationManager() {
        if (controller.getButtonPressed("touchpad")) {
            Vector3 forward = transform.TransformDirection(Vector3.forward) * teleportDistance;
            Debug.DrawRay(transform.position, forward * 200, Color.green);
        }
        if (controller.getButtonPressed("touchpad") && (!cameraRig.isTeleporting || iOwnPS)) {
            //tce.transform.position = transform.position;
            if (!tce.isPlaying) {
                tce.Play();
                //Debug.Log(transform.name + " controller began particle emmission.");
            }
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
            tce.Stop();
            //Debug.Log(transform.name + " controller stopped particle emmission.");
            iOwnPS = false;
            Vector3 pos = ps.transform.position;
            pos.y = cameraRig.terrain.SampleHeight(pos) + cameraRig.terrainHeight;
            Instantiate(marker).transform.position = pos;
            pos.y = (cameraRig.terrain.SampleHeight(pos) + cameraRig.terrainHeight) - Config.HMDStadningHeight/2.0f + Config.godTeleportationHeightOffset;
            Vector3 offset = hmd.transform.localPosition;
            offset.y = 0;
            pos -= offset * transform.parent.localScale.x;
            transform.parent.parent.position = pos;
        }
    }

    void grabManager() {
        if(objectToGrab != null) {
            if (controller.getButtonPressed("trigger")) {
                objectToGrab.transform.parent = transform;
                objectToGrab.GetComponent<Rigidbody>().useGravity = false;
                objectToGrab.GetComponent<Rigidbody>().velocity = Vector3.zero;
            } else if (objectToGrab.transform.parent == transform) {
                objectToGrab.transform.parent = null;
                objectToGrab.GetComponent<Rigidbody>().useGravity = true;
				objectToGrab.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
				objectToGrab.GetComponent<Rigidbody>().velocity = controller.getVelocity();
				objectToGrab.GetComponent<Grabbable>().snapToGrid();
            }
        }
    }

    void possessionManager() {
        if (objectToPossess != null) {
            if (controller.getButtonPressed("grip")) {
                if (pe.isStopped) {
                    posessionTimer = objectToPossess.GetComponent<Possessable>().timeToPossess;
                    pe.Play();
                }
                posessionTimer -= Time.deltaTime;
                if(posessionTimer <= 0) {
                    pe.Emit(50);
                    possessing = false;
                    pe.Stop();
                    possess();
                }
            }else if (pe.isPlaying) {
                pe.Stop();
            }
        }else if (possessing) {
            pe.Stop();
        }
    }

    void possess() {
        //Do an animation. That would be cool
        Possessable p = objectToPossess.GetComponent<Possessable>();
        //float modelHeight = p.getModelHeight();
        Transform newHMDLocation = p.getPreferredHMDPosition();
        float height = newHMDLocation.localPosition.y * p.getModelScale().y;
        float scale = height/Config.HMDStadningHeight;
        scale *= Config.godScale;
        Vector3 playAreaScale = new Vector3(scale, scale, scale);
        transform.parent.localScale = playAreaScale;

        //This will need to be changed when we finally have real models.
        //transform.parent.parent.position = p.transform.position;
        transform.parent.parent = objectToPossess.transform;    //change parent to be the monster.
        transform.parent.localPosition = Vector3.zero;
        transform.parent.localEulerAngles = Vector3.zero;
        Possessed possessed = p.gameObject.AddComponent<Possessed>();
        possessed.setHeadTransform(cameraRig.transform.FindChild("Camera (eye)").transform);
        cameraRig.possessionBody = p.gameObject;
        transform.parent.BroadcastMessage("switchControl", "possession");
    }

    void calibrationManager() {
        if (controller.getButtonDown("menu")) {
            Transform hmd = transform.parent.FindChild("Camera (eye)").transform;
            float scale = transform.parent.lossyScale.y;
            Config.HMDStadningHeight = hmd.localPosition.y * scale;
            Debug.Log("Height Calibrated to " + Config.HMDStadningHeight);
        }
    }

    public void switchControl(string control) {
        switch (control) {
            case "possession":
                this.enabled = false;
                break;
            case "god":
                this.enabled = true;
                break;
        }
    }
}
