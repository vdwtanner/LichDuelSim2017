using UnityEngine;
using System.Collections;

public class UIButton : MonoBehaviour {
    public delegate void OnTriggerDown(UIButton button);
    public delegate void OnPointerEnter(UIButton button);
    public OnTriggerDown onTriggerDown;
    public OnPointerEnter onPointerEnter;
	public string tooltipText = "";
	private TextMesh textArea;

	// Use this for initialization
	void Start () {
		textArea = transform.parent.parent.parent.FindChild("TooltipTextArea").GetComponent<TextMesh>();
	}
	
    public void OnPointerIn(Controller controller) {
        if(onPointerEnter != null) {
            onPointerEnter(this);
        }
    }

	public void OnPointerStay(Controller controller) {
		textArea.text = tooltipText;
        if(onTriggerDown != null) {
            if (controller.getButtonDown("trigger")) {
                onTriggerDown(this);
            }
        }
    }
}
