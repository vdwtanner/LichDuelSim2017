using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Entity))]
public class Grabbable : MonoBehaviour {
	
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
            //Debug.Log("Player exited grabbable region.");
            Controller controller = other.GetComponent<Controller>();
            if(gc.objectToGrab == gameObject && transform.parent != gc.transform) {
                gc.objectToGrab = null;
            }
        }
	}

}
