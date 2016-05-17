using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Collider))]
public class Grabbable : MonoBehaviour {
    public Collider triggerRegion;
	// Use this for initialization
	void Start () {
        triggerRegion = GetComponent<Collider>();
	}
	
	void OnTriggerEnter(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Controller controller = other.GetComponent<Controller>();
            controller.hapticPulse(1500);
            gc.objectToGrab = gameObject;
        }
    }

    void OnTriggerExit(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Debug.Log("Player entered grabbable region.");
            Controller controller = other.GetComponent<Controller>();
            controller.hapticPulse(1200);
            if(gc.objectToGrab == gameObject) {
                gc.objectToGrab = null;
            }
        }
    }
}
