using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;

    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    private Valve.VR.EVRButtonId touchpadButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
	}

    public bool getButtonDown(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPressDown(gripButton);
            case "trigger":
                return controller.GetPressDown(triggerButton);
            case "touchpad":
                return controller.GetPressDown(touchpadButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    public bool getButtonUp(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPressUp(gripButton);
            case "trigger":
                return controller.GetPressUp(triggerButton);
            case "touchpad":
                return controller.GetPressUp(touchpadButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    public bool getButtonPressed(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPress(gripButton);
            case "trigger":
                return controller.GetPress(triggerButton);
            case "touchpad":
                return controller.GetPress(touchpadButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    public void hapticPulse(ushort duration) {
        controller.TriggerHapticPulse(duration);
    }
}
