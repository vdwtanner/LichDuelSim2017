using UnityEngine;
using System.Collections;

public class UISlider : MonoBehaviour {
    public delegate void OnTriggerDown(UISlider slider);
    public delegate void OnPointerEnter(UISlider slider);
    public delegate void OnPointerDrag(UISlider slider);
	public delegate void OnTriggerUp(UISlider slider);
	public OnTriggerDown onTriggerDown;
    public OnPointerEnter onPointerEnter;
    public OnPointerDrag onPointerDrag;
	public OnTriggerUp onTriggerUp;
    public bool isVertical = true;
	private bool draggingSlider = false;
	/// <summary>
	/// This is good for getting closer to the specified min and max values. LArger sliders have a harder time reaching min and max due to the implementation.
	/// </summary>
    public bool useSmoothStep = true;
    private float value = .5f;
    public float min = 0;
    public float max = 1.0f;
	public float calculatedValue;
	public string tooltipText = "";
	private TextMesh textArea;

	public Transform slider;

    // Use this for initialization
    void Start () {
		calculatedValue = calcValue();
		textArea = transform.parent.parent.parent.FindChild("TooltipTextArea").GetComponent<TextMesh>();
	}

    public void OnPointerIn(Controller controller) {
        if (onPointerEnter != null) {
            onPointerEnter(this);
        }
    }

    public void OnPointerStay(Controller controller) {
		textArea.text = tooltipText;
        if (controller.getButtonDown("trigger")) {
			draggingSlider = true;
            if(onTriggerDown != null) {
                onTriggerDown(this);
            }
        }
        if (controller.getButtonPressed("trigger") && draggingSlider) {
			if(onPointerDrag != null) {
				onPointerDrag(this);
			}
			slider.position = controller.laserPointer.collisionPoint;
			setValue(slider.localPosition.x+.5f);
        }
		if (controller.getButtonUp("trigger")) {
			draggingSlider = false;
			if (onTriggerUp != null) {
				onTriggerUp(this);
			}
		}
	}

	public void OnPointerOut(Controller controller) {
		if (draggingSlider) {
			controller.onTriggerRelease += OnDragTriggerRelease;
		}
	}

	public void OnDragTriggerRelease(Controller controller) {
		draggingSlider = false;
		if (onTriggerUp != null) {
			onTriggerUp(this);
		}
	}

    public float calcValue() {
        if (useSmoothStep) {
            return Mathf.SmoothStep(min, max, value);
        }
        return Mathf.Lerp(min, max, value);
    }

    public void setValue(float value) {
        this.value = value;
		Vector3 pos = slider.localPosition;
		pos.x = Mathf.Clamp(value - .5f, -.5f, .5f);
		pos.y = 0;
		pos.z = 0;
		slider.localPosition = pos;
		calculatedValue = calcValue();
    }

	/// <summary>
	/// Calculates where to place the slider given a real value between min and max for this UISlider. Does not take smooth stepping into account because that function is rather large and painful. This is a decent approximation though. (inverse smooth step is = the inverse of x^2(3-2x).)
	/// </summary>
	/// <param name="value"></param>
	public void setCalculatedValue(float value) {
		setValue(Mathf.InverseLerp(min, max, value));
	}

	public void setIsVertical(bool isVertical) {
		this.isVertical = isVertical;
		if (!isVertical) {
			transform.localEulerAngles = new Vector3(0, 0, 90);
		} else {
			transform.localEulerAngles = Vector3.zero;
		}
	}
}
