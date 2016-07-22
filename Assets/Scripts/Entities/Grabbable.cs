using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Entity))]
public class Grabbable : MonoBehaviour {
	private Entity h_entity;

	void Start() {
		h_entity = GetComponent<Entity>();
	}
	
	void OnTriggerEnter(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Controller controller = other.GetComponent<Controller>();
            controller.hapticPulse(1500);
			if(gc.grabbing == false) {
				if(gc.objectToGrab != null) {
					gc.objectToGrab.GetComponent<Entity>().removeSelectionHex();
				}
				gc.objectToGrab = gameObject;
				//TODO add logic to only do this for the correct team
				h_entity.showSelectionHex();
			}
		}
    }

    void OnTriggerExit(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            //Debug.Log("Player exited grabbable region.");
            //Controller controller = other.GetComponent<Controller>();
            if(gc.objectToGrab == gameObject && transform.parent != gc.transform) {
                gc.objectToGrab = null;
				h_entity.removeSelectionHex();
			}
		}
	}

	public Entity getEntity() {
		return h_entity;
	}

}
