using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {
    public delegate void OnTriggerDown(UIButton button);
    public delegate void OnPointerEnter(UIButton button);
    public OnTriggerDown onTriggerDown;
    public OnPointerEnter onPointerEnter;

     
	// Use this for initialization
	void Start () {
	}
	
    public void OnPointerIn(Controller controller) {
        if(onPointerEnter != null) {
            onPointerEnter(this);
        }
    }

	public void OnPointerStay(Controller controller) {
        if(onTriggerDown != null) {
            if (controller.getButtonDown("trigger")) {
                onTriggerDown(this);
            }
        }
    }
}
