using UnityEngine;
using System.Collections;

public class AlwaysFacePlayer : MonoBehaviour {
	[Tooltip("This should be the camera of the player you want to always face.")]
	public Transform target;

	// Update is called once per frame
	void FixedUpdate() {
		Vector3 lookPos = target.position - transform.position;
		lookPos.y = 0;
		transform.rotation = Quaternion.LookRotation(lookPos);
		transform.RotateAround(transform.position, transform.up, 180);
	}
}
