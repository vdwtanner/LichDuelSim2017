using UnityEngine;
using System.Collections;

//[RequireComponent (typeof (Collider))]
public class Grabbable : MonoBehaviour {
	public bool m_snapToGrid;
	private bool m_snapRequested;
	private bool m_onTerrain = false;
	
	void OnTriggerEnter(Collider other) {
        GodController gc = other.GetComponent<GodController>();
        if (gc != null) {
            Controller controller = other.GetComponent<Controller>();
            controller.hapticPulse(1500);
            gc.objectToGrab = gameObject;
		}
    }

	void OnCollisionEnter(Collision collision) {
		Collider other = collision.collider;
		Terrain t = other.GetComponent<Terrain>();
		if (t != null) {
			m_onTerrain = true;
			if (!m_snapRequested) {
				return;
			}
			TerrainHexGrid thg = t.GetComponent<TerrainHexGrid>();
			if (thg == null) {
				Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
				throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
			}
			Hex hex = thg.getHexFromWorldPos(transform.position);
			if (hex.isValid()) {
				snapTo(hex.worldPosition);
			} else {
				Debug.LogWarning("Invalid snap location");
			}
		}
	}

	void OnCollisionExit(Collision collision) {
		Collider other = collision.collider;
		Terrain t = other.GetComponent<Terrain>();
		if (t != null) {
			m_onTerrain = false;
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

	public void snapToGrid() {
		//Debug.Log("Snap to grid called");
		if (m_snapToGrid) {
			//Debug.Log("requesting to snap");
			if (m_onTerrain) {
				//TODO use a faster implementation of this
				TerrainHexGrid thg = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainHexGrid>();
				if (thg == null) {
					Debug.LogError("The Terrain doesn't have a TerrainHexGrid");
					throw new MissingComponentException("The Terrain doesn't have a TerrainHexGrid component attached!");
				}
				Hex hex = thg.getHexFromWorldPos(transform.position);
				if (hex.isValid()) {
					snapTo(hex.worldPosition);
					m_snapRequested = false;
				} else {
					Debug.LogWarning("Invalid snap location");
				}
			}
			m_snapRequested = true;
		}
	}

	private void snapTo(Vector3 pos) {
		transform.position = pos;
		Vector3 rot = new Vector3(0, transform.eulerAngles.y, 0);
		transform.eulerAngles = rot;
		Rigidbody body = GetComponent<Rigidbody>();
		if(body != null) {
			body.velocity = Vector3.zero;
			body.useGravity = false;
			//body.freezeRotation = true;
			body.constraints = RigidbodyConstraints.FreezeAll;
		}
	}

}
