using UnityEngine;
using System.Collections.Generic;

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

    public VRHelper vrHelper { get; private set; }

    //Text areas
    private Dictionary<string, float> textTimers;
    private bool checkTextTimers = false;
    private List<string> timerKeys;

    //Laser
    public LaserPointer laserPointer { get; private set; }
    public Color laserPointerColor = Color.green;

    //Happens before Start(), which is good for functions that want to tell the controller to show the mouse wheel in their start functions.
    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        renderModel = transform.GetChild(0).GetComponent<SteamVR_RenderModel>();//This should be the Model GameObject
        initTextTimers();
        laserPointer = gameObject.AddComponent<LaserPointer>();
        laserPointer.color = laserPointerColor;
        laserPointer.showLaserOnStart = false;
        vrHelper = transform.parent.GetComponent<VRHelper>();
    }

    // Use this for initialization
    void Start () {
        
    }

    void initTextTimers() {
        textTimers = new Dictionary<string, float>();
		Transform model = transform.FindChild("Model");
		for(int x = 0; x < model.childCount; x++) {
			Transform attachPoint = model.GetChild(x).FindChild("attach");
			if (attachPoint) {
				Transform textArea = attachPoint.FindChild("TextArea");
				if (textArea) {
					textTimers.Add(attachPoint.parent.name, 0);
				}
			}
		}
        timerKeys = new List<string>(textTimers.Keys);
    }

    void FixedUpdate() {
        triggerAxis = controller.GetAxis(triggerButton);
        thumbOnTouchpad = controller.GetTouch(touchpadButton);
        touchpadAxis = controller.GetAxis();
        scrollWheelManager();
        if (checkTextTimers) {
            textTimerManager();
        }
           
    }

    void textTimerManager() {
        bool dirty = false;
        foreach (string key in timerKeys) {
            if(textTimers[key] > 0) {
                textTimers[key] -= Time.fixedDeltaTime;
                if(textTimers[key] > 0) {
                    dirty = true;
                } else {
                    transform.GetChild(0).FindChild(key).FindChild("attach").FindChild("TextArea").GetComponent<Renderer>().enabled = false;
                }
            }
        }
        checkTextTimers = dirty;
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

    /// <summary>
    /// Enable or disable the scroll wheel
    /// </summary>
    /// <param name="show"></param>
    public void showScrollWheel(bool show) {
        renderModel.controllerModeState.bScrollWheelVisible = show;
        scrollWheelShown = show;
    }

    /// <summary>
    /// Show text at location for duration seconds. Returns false if the specified location isn't valid.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public bool showText(string text, string location, float duration) {
        if (!textTimers.ContainsKey(location)) {
            return false;
        }
        Transform textArea = transform.GetChild(0).FindChild(location).FindChild("attach").FindChild("TextArea");
        textArea.GetComponent<TextMesh>().text = text;
        textArea.GetComponent<Renderer>().enabled = true;
        textTimers[location] = duration;
        checkTextTimers = true;
        return true;
    }

    /// <summary>
    /// Turn the laserPointer on and off with this method.
    /// </summary>
    /// <param name="enabled"></param>
    public void enableLaserPointer(bool enabled) {
        laserPointer.pointer.GetComponent<Renderer>().enabled = enabled;
    }

    public RaycastHit getLaserPointerRaycastHit() {
        return laserPointer.hit;
    }
}
