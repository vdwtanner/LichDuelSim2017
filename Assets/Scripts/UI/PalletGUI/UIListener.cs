using UnityEngine;
using System.Collections;
/// <summary>
/// This class exists purely to pass along signals among stacked elements, eg. the slider element that accompanies a UISlider
/// </summary>
public class UIListener : MonoBehaviour {
	public delegate void OnPointerEnter(Controller controller);
	public delegate void OnPointerRemain(Controller controller);
	public delegate void OnPointerExit(Controller controller);
	public OnPointerEnter onPointerEnter;
	public OnPointerRemain onPointerRemain;
	public OnPointerExit onPointerExit;

	public void OnPointerIn(Controller controller) {
		if (onPointerEnter != null) {
			onPointerEnter(controller);
		}
	}

	public void OnPointerStay(Controller controller) {
		if (onPointerRemain != null) {
			onPointerRemain(controller);
		}
	}

	public void OnPointerOut(Controller controller) {
		if(onPointerExit != null) {
			onPointerExit(controller);
		}
	}
}
