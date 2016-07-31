using UnityEngine;

[RequireComponent (typeof(Collider))]
public class LightSwitchInteraction : MonoBehaviour {

	private Controller controller;
	private Light light;
	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller>();
	}
	
	// Update is called once per frame
	void Update () {
		if (light!= null && controller.getButtonDown("trigger")) {
			light.enabled = !light.enabled;
		}
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag == "LightSwitch") {
			light = other.GetComponent<Light>();
			controller.hapticPulse(1000);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == "LightSwitch" && other.GetComponent<Light>() == light) {
			light = null;
		}
	}
}
