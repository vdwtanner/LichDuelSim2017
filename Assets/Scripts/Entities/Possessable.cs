using UnityEngine;
using System.Collections;

public class Possessable : MonoBehaviour {
    [Tooltip("Time in seconds")]
    public float timeToPossess;

    bool pulseEnabled;

    void Start() {
        
        if(GetComponent<Grabbable>() == null) {
            pulseEnabled = true;
        } else {
            pulseEnabled = false;
        }
    }

    void OnTriggerEnter(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null && !gc.possessing) {
            Controller controller = other.GetComponent<Controller>();
            if (pulseEnabled) {
                controller.hapticPulse(1500);
            }
            gc.objectToPossess = gameObject;
        }
    }

    void OnTriggerExit(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Debug.Log("Player entered possessable region.");
            Controller controller = other.GetComponent<Controller>();
            if (gc.objectToPossess == gameObject) {
                gc.objectToPossess = null;
            }
        }
    }

    public float getModelHeight() {
        return GetComponent<Renderer>().bounds.size.y;
    }

    public Transform getPreferredHMDPosition() {
        return transform.FindChild("HMD");
    }

    public Vector3 getModelScale() {
        return transform.localScale;
    }
}
