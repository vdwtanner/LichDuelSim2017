using UnityEngine;
using System.Collections;

public class FreeCamera : MonoBehaviour {

    public float speed = 1.0f;
    public float rotationSpeed = 5.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Cursor.visible = false;
        // TRANSLATION
        Vector3 trans = new Vector3();
        trans.x = Input.GetAxis("Horizontal");
        trans.z = Input.GetAxis("Vertical");
        //trans.y = Input.GetAxis("AscendDescend");
        trans *= speed;
        transform.Translate(trans.x, 0, trans.z, Space.Self);
        transform.Translate(0, trans.y, 0, Space.World);

        // ROTATION
        pitch += -Input.GetAxisRaw("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90, 90);
        yaw += Input.GetAxisRaw("Mouse X");
        transform.eulerAngles = new Vector3(pitch, yaw, 0) * rotationSpeed;

		Cursor.lockState = CursorLockMode.Locked;
    }
}
