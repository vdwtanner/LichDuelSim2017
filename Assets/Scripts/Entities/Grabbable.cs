using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Collider))]
public class Grabbable : MonoBehaviour {
	public bool m_snapToGrid { get; set; }
	private bool m_snapRequested;
	
	void Update() {
		
	}

	void OnTriggerEnter(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Controller controller = other.GetComponent<Controller>();
            controller.hapticPulse(1500);
            gc.objectToGrab = gameObject;
		} else {
			HexChunk hc = other.GetComponent<HexChunk>();
			if(hc != null) {
				Vector2 hexLoc = hc.getTerrainHexGrid().GetHexIndexFromWorldPos(transform.position);
			}
		}
    }

    void OnTriggerExit(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Debug.Log("Player exited grabbable region.");
            Controller controller = other.GetComponent<Controller>();
            if(gc.objectToGrab == gameObject) {
                gc.objectToGrab = null;
            }
        }
    }

	public bool snapToGrid() {
		if (m_snapToGrid) {
			m_snapRequested = true;
			return true;
		}
		return false;
	}

}
