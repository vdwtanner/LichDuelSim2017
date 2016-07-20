using UnityEngine;
using System.Collections;

public class HexInvalidationTool : EditorTool {

	public override void OnSelection() {
		if (hController != null) {
			hController.enableLaserPointer(true);
		}
	}

	public override void OnUnselect() {

	}

	public override void BrushAltFire() {
		if (hController != null) {
			hController.enableLaserPointer(true);
		}

	}

	public override void BrushAltFireUp() {

	}

	public override void BrushPrimaryFire() {
		if (hController != null) {
			hController.enableLaserPointer(true);
		}
		if (getHitTerrain() != null) {
			getHexGrid().SetHexValid(getHit().point, false, true);
		}
	}

}
