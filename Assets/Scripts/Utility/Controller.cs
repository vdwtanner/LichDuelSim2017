using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;

    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    private Valve.VR.EVRButtonId touchpadButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

    private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_RenderModel renderModel;

    private Vector2 triggerAxis;
    private Vector2 touchpadAxis;
    private Vector2 scrollWheelAxis = Vector2.zero;
    private bool thumbOnTouchpad = false;

    private Transform scrollwheel;
    private bool scrollWheelShown = false;
    private float scrollWheelStartRotation = 0;
    private bool setStartRotation = false;


    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        renderModel = transform.GetChild(0).GetComponent<SteamVR_RenderModel>();//This should be the Model GameObject
        
	}

    void FixedUpdate() {
        triggerAxis = controller.GetAxis(triggerButton);
        thumbOnTouchpad = controller.GetTouch(touchpadButton);
        touchpadAxis = controller.GetAxis();
        scrollWheelManager();        
    }

    void scrollWheelManager() {
        if (scrollWheelShown) {
            if (scrollwheel != null) {
                if (getTouchDown("touchpad") || setStartRotation) {
                    setStartRotation = !setStartRotation;
                    scrollWheelStartRotation = scrollwheel.localEulerAngles.x;
                    if (scrollWheelStartRotation > 180) {
                        scrollWheelStartRotation -= 360;
                    }
                }
                if(scrollWheelStartRotation != 0 && getTouchUp("touchpad")){
                    scrollWheelStartRotation = 0;
                } else {
                    scrollWheelAxis.x = scrollwheel.localEulerAngles.x;
                    if (scrollWheelAxis.x > 180) {
                        scrollWheelAxis.x -= 360;
                    }
                    scrollWheelAxis.x -= scrollWheelStartRotation;
                }
                
            } else {
                scrollwheel = transform.GetChild(0).FindChild("scroll_wheel");
                scrollWheelAxis.x = 0;
            }
        } else {
            scrollWheelAxis.x = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName">Can be grip, trigger, touchpad, or menu</param>
    /// <returns></returns>
    public bool getButtonDown(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPressDown(gripButton);
            case "trigger":
                return controller.GetPressDown(triggerButton);
            case "touchpad":
                return controller.GetPressDown(touchpadButton);
            case "menu":
                return controller.GetPressDown(menuButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName">Can be grip, trigger, touchpad, or menu</param>
    /// <returns></returns>
    public bool getButtonUp(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPressUp(gripButton);
            case "trigger":
                return controller.GetPressUp(triggerButton);
            case "touchpad":
                return controller.GetPressUp(touchpadButton);
            case "menu":
                return controller.GetPressUp(menuButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName">Can be grip, trigger, touchpad, or menu</param>
    /// <returns></returns>
    public bool getButtonPressed(string buttonName) {
        switch (buttonName) {
            case "grip":
                return controller.GetPress(gripButton);
            case "trigger":
                return controller.GetPress(triggerButton);
            case "touchpad":
                return controller.GetPress(touchpadButton);
            case "menu":
                return controller.GetPress(menuButton);
            default:
                Debug.LogWarning("Invalid button requested: " + buttonName);
                return false;
        }
    }

    public bool getTouchDown(string name) {
        switch (name) {
            case "touchpad":
                return controller.GetTouchDown(touchpadButton); ;
        }
        return false;
    }

    public bool getTouch(string name) {
        switch (name) {
            case "touchpad":
                return controller.GetTouchDown(touchpadButton); ;
        }
        return false;
    }

    public bool getTouchUp(string name) {
        switch (name) {
            case "touchpad":
                return controller.GetTouchUp(touchpadButton);
        }
        return false;
    }

    /// <summary>
    /// Axis are not reset to zero until the player takes their finger off of the device.
    /// </summary>
    /// <param name="axisName">Can be trigger, touchpad, or scrollwheel</param>
    /// <returns></returns>
    public Vector2 getAxis(string axisName) {
        switch (axisName) {
            case "trigger":
                return triggerAxis; ;
            case "touchpad":
                return touchpadAxis;
            case "scrollWheel":
                return scrollWheelAxis;
        }
        return touchpadAxis;
    }

    public void hapticPulse(ushort duration) {
        controller.TriggerHapticPulse(duration);
    }

    public Vector3 getVelocity() {
        Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
        return origin.TransformVector(controller.velocity);
    }

    public Vector3 getAngularVelocity() {
        Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
        return origin.TransformVector(controller.angularVelocity);
    }

    public void showScrollWheel(bool show) {
        renderModel.controllerModeState.bScrollWheelVisible = show;
        scrollWheelShown = show;
    }
}
